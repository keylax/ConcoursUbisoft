let app = require('express')();
let http = require('http').createServer(app);
let schedule = require('node-schedule');
let io = require('socket.io')(http);
let activePolls = [];
let rooms = [];
let timeout;
let openedConnections = 0;

schedule.scheduleJob('0 0 * * *', () => {
  io.emit('endClientSidePoll');
  activePolls = [];
  console.log("Hi, it is midnight and you know what that means.")
});

io.on('connection', function(socket){
  openedConnections++;
  //New room was created by the game
  socket.on('newRoom', function (roomDetails) {
    /*let roomCode = roomDetails.roomCode;
    if (!roomExists(roomCode)) {
      rooms.push({roomName: roomCode, nbrOfClients: 0});
      console.log("Room " + roomCode + " was opened by the game");
    }
    console.log(rooms);*/
  });

  socket.on('gameSocketJoin', function(roomDetails) {
    let roomCode = roomDetails.roomCode;
    endActivePoll(roomCode);
    socket.join(roomCode);
  });

  //The game was interrupted and the room is closed
  socket.on('closeRoom', function (roomDetails) {
    let roomCode = roomDetails.roomCode;
    socket.leave(roomCode);
    /*closeRoom(roomCode);
    io.sockets.in(roomCode).emit('closeRoom');
    clearRoom(roomCode);
    console.log("Room " + roomCode + " was closed by the game");*/
  });

  //User joins a room
  socket.on('joinRoom', function(room) {
    socket.join(room);
    if (isPollActiveInRoom(room)) {
      socket.emit('pollInProgress', {'activePoll': getActivePollInRoom(room)});
    }
    /*if(roomExists(room)) {
      console.log("(Success) User join room " + room);
      socket.join(room);
      addPlayerToRoom(room);
      console.log(rooms);
    }*/
  });

  //User leaves room
  socket.on('leaveRoom', function(room, currentVote){
    socket.leave(room);
    if (currentVote !== null) {
      substractVote(room, currentVote);
      io.sockets.in(room).emit('updateVotes', {'pollStatus': getPollStatus(room)});
    }
   /*if(roomExists(room)) {
      console.log("User left room " + room);
      socket.leave(room);
      removePlayerFromRoom(room);
      console.log(rooms);
    }*/
  });

  //Game starts a poll
  socket.on('poll', function(pollDetails){
    if(!isPollActiveInRoom(pollDetails.roomName)) {
      console.log("Starting a " + pollDetails.duration + " seconds poll in room: " + pollDetails.roomName);
      startPoll(pollDetails);
      io.sockets.in(pollDetails.roomName).emit('poll', {'pollDetails': pollDetails});

      if (pollDetails.duration >= 0) {
        timeout = setTimeout(() => getPollResult(pollDetails.roomName), pollDetails.duration * 1000);
      }
    }
  });

  //Manually end poll
  socket.on('endPoll', function(roomDetails) {
    getPollResult(roomDetails.roomCode)
    clearTimeout(timeout);
  });

  //Client asks server if it is ok to join the room
  socket.on('isRoomValid', function(room, isValid) {
    isValid(true);
    if (roomExists(room)) {
      isValid(true);
    } else {
      isValid(false);
    }
  });

  //When a user first selects an option.
  socket.on('voteCast', function(room, option) {
    addVote(room, option);
    io.sockets.in(room).emit('updateVotes', {'pollStatus': getPollStatus(room)});
  });

  //When a user changes his vote.
  socket.on('voteChange', function(room, previousOption, newOption) {
    changeVote(room, previousOption, newOption);
    io.sockets.in(room).emit('updateVotes', {'pollStatus': getPollStatus(room)});
  });
});

//Does the room exist
function roomExists(roomCode) {
  for  (let room of rooms) {
    if (room.roomName === roomCode) {
      return true;
    }
  }
  return false;
}

//Add new poll to activePolls
function startPoll(pollDetails) {
  let votesArray = [];
  for(let opt in pollDetails.options){
    votesArray.push(0);
  }
  activePolls.push({roomName: pollDetails.roomName, duration: pollDetails.duration, options: pollDetails.options, votes: votesArray})
}

//Add vote to one of the options of a poll
function addVote(room, option) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      activePolls[i].votes[option]++;
      io.sockets.in(room).emit('updateVotesGame', {"votes": activePolls[i].votes});
    }
  }
}

//Substract vote from one of the options of a poll
function substractVote(room, option) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      activePolls[i].votes[option]--;
      io.sockets.in(room).emit('updateVotesGame', {"votes": activePolls[i].votes});
    }
  }
}

//A user changed his vote, substract 1 from a certain option and add 1 to another.
function changeVote(room, previousOption, newOption) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      activePolls[i].votes[previousOption]--;
      activePolls[i].votes[newOption]++;
      io.sockets.in(room).emit('updateVotesGame', {"votes": activePolls[i].votes});
    }
  }
}

//Return the current vote status for a poll.
function getPollStatus(room) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      return activePolls[i].votes;
    }
  }
}

//Return if there is already a poll in progress in the room.
function isPollActiveInRoom(room) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      return true;
    }
  }
  return false;
}

//Get active poll in room
function getActivePollInRoom(room) {
  for (let i = 0, len = activePolls.length; i < len; i++) {
    if (activePolls[i].roomName === room) {
      return activePolls[i];
    }
  }
}

//A poll has ended, send the result back to the game and call endPoll.
function getPollResult(room) {
  for (let i = activePolls.length - 1; i >= 0; i--) {
    if (activePolls[i].roomName === room) {
      let result = getWinningVote(activePolls[i].votes);
      io.sockets.in(room).emit('result', {'result': result});
      io.sockets.in(room).emit('endClientSidePoll');
      activePolls.splice(i, 1);
    }
  }
}

//Return the winning option. Choose a random winning amongst duplicates.
function getWinningVote(votes) {
  let winningVote;
  let duplicates = [];

  winningVote = votes.indexOf(Math.max(...votes));
  for (let i = 0, len = votes.length; i < len; i++) {
    if (votes[i] === votes[winningVote]) {
      duplicates.push(i);
    }
  }

  if (duplicates.length > 1) {
    winningVote =  duplicates[Math.floor(Math.random()*duplicates.length)];
  }

  return winningVote
}

function endActivePoll(room) {
  for (let i = activePolls.length - 1; i >= 0; i--) {
    if (activePolls[i].roomName === room) {
      clearTimeout(timeout);
      io.sockets.in(room).emit('endClientSidePoll');
      activePolls.splice(i, 1);
    }
  }
}

//Check if a specific room exists
function closeRoom(roomCode) {
  if (rooms.length === 0) return;
  if (rooms.length === 1) {
    rooms.pop();
  } else {
    for( let i = 0; i < rooms.length; i++){
      if (rooms[i].roomName === roomCode) {
        rooms.splice(i, 1);
      }
    }
  }
}

//Remove poll that just finished from the activePolls.
function endPoll(room, pollIndex) {
  io.sockets.in(room).emit('endClientSidePoll');
  if (activePolls.length === 0) return;
  if (activePolls.length === 1) {
    activePolls.pop();
  } else {
    activePolls.splice(pollIndex, 1);
  }
}


//Player joined room
function addPlayerToRoom(room) {
  for  (let aRoom of rooms) {
    if (aRoom.roomName === room) {
      aRoom.nbrOfClients = aRoom.nbrOfClients + 1;
    }
  }
}

//Player left room
function removePlayerFromRoom(room) {
  for  (let aRoom of rooms) {
    if (aRoom.roomName === room) {
      aRoom.nbrOfClients = aRoom.nbrOfClients - 1;
    }
  }
}

//Nbr of sockets connected to a room
function getNbrOfSockets(room) {
  let socketList = io.of('/').in(room).sockets;
  return Object.keys(socketList).length;
}

//When a room is closed, clear all sockets in room
function clearRoom(room) {
  io.of('/').in('chat').clients((error, socketIds) => {
    socketIds.forEach(socketId => io.sockets.sockets[socketId].leave(room));
  });
}

//Prints all sockets in a room. For debug purposes.
function printClients(room) {
  io.of('/').in(room).clients(function(error,clients){
    if (clients.length === 0) {
      io.broadcast.emit('endClientSidePoll');

    }
  });
}

http.listen(3000);
console.log('listening on port 3000')
