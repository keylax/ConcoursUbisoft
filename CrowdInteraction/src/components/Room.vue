<template>
<div>
  <div>
    <md-chip class="room-name-chip center">Room: {{roomCode}}</md-chip>
  </div>
  <div class="poll">
    <div class="optDiv">
      <md-list class="md-double-line" style="background-color: #fafafa">
        <md-list-item v-for="(pollOption, index) of pollOptions" :key="index">
          <md-radio class="opt-font" v-model="selectedPollOptionValue" v-on:change="voteChanged" :value="index">{{pollOption.option}}</md-radio>
        </md-list-item>
      </md-list>
    </div>
    <div class="voteDiv">
      <md-list class="md-double-line" style="background-color: #fafafa">
        <md-list-item v-for="(pollVote, index) of pollVoteStatus" :key="index">
          <p class="vote-percentage">{{pollVote}} %</p>
        </md-list-item>
      </md-list>
    </div>
  </div>
</div>
</template>

<script>
    export default {
        name: "Room",
      data() {
          return {
            msg: '',
            pollOptions: [],
            pollVoteStatus: [],
            selectedPollOptionValue: null,
            roomCode: '',
            errorMsg: '',
            returning: false,
            previousSelected: null,
            checkConnectionTimer: null,
          }
      },

      sockets: {
        poll(pollDetails) {
          this.initPoll(pollDetails.pollDetails);
        },

        pollInProgress(pollDetails) {
          this.displayActivePoll(pollDetails.activePoll);
        },

        endClientSidePoll() {
          this.endPoll()
        },

        closeRoom() {
          this.errorMsg = 'The room was closed. Returning you to the main page.'
          setTimeout(() => this.$router.push("/"), 3500);
        },

        updateVotes(voteStatus) {
          this.displayVotesAsPercentage(voteStatus.pollStatus);
        },
      },

      created() {
        this.roomCode = this.$route.params.valueOf().roomcode;
        //this.checkConnectionTimer = setInterval( () => this.checkConnection(), 10000);
        window.addEventListener('beforeunload', this.beforePageDestroyed)
        this.joinRoom();
      },

      beforeDestroy() {
          clearInterval(this.checkConnectionTimer);
          this.leaveRoom();
      },

      methods: {

        displayVotesAsPercentage(votes) {
          let newVoteStatus = [];
          let totalVotes = 0;

          for (let i = 0, len = votes.length; i < len; i++) {
            let zero = parseFloat(0).toFixed(1);
            newVoteStatus.push(zero);
            totalVotes = totalVotes + votes[i];
          }

          if (totalVotes !== 0) {
            for (let i = 0, len = votes.length; i < len; i++) {
              let percentageOfVotes = votes[i] / totalVotes * 100;
              newVoteStatus[i] = parseFloat(Math.round(percentageOfVotes * 100) / 100).toFixed(1);
            }
          }
          this.pollVoteStatus = newVoteStatus;
        },

        initPoll(pollDetails) {
          this.pollOptions = pollDetails.options;
          let initialVoteStatus = [];
          for (let i = 0, len = pollDetails.options.length; i < len; i++) {
            let zero = parseFloat(0).toFixed(1);
            initialVoteStatus.push(zero);
          }
          this.pollVoteStatus = initialVoteStatus;
        },

        displayActivePoll(pollDetails) {
          this.pollOptions = pollDetails.options;
          this.displayVotesAsPercentage(pollDetails.votes);
        },

        endPoll() {
          this.resetData();
        },

        leaveRoom() {
          this.$socket.emit('leaveRoom', this.roomCode, this.selectedPollOptionValue);
          this.resetData();
          this.roomCode = "";
          this.errorMsg = '';
        },

        joinRoom() {
          this.$socket.emit('joinRoom', this.roomCode);
        },

        beforePageDestroyed: function (event) {
          this.leaveRoom();
        },

        checkConnection() {
          /*if (this.$socket.connected) {
            if (this.returning === false) {
              this.checkRoomStatus();
            }
          } else {
            this.errorMsg = "Connection to the server was lost. Returning you to the main page."
            console.log("wat");
            setTimeout(() => this.$router.push("/"), 3500);
          }*/

          if (!this.$socket.connected) {
            this.errorMsg = "Not connected to the server."
          }
        },

        checkRoomStatus() {
          this.$socket.emit('isRoomValid', this.roomCode, (response) => {
            if (!response) {
              this.errorMsg = "This room no longer exists."
              setTimeout(() => this.$router.push("/"), 3500);
            }
          });
        },

        voteChanged() {
          if (this.previousSelected === null){
            this.previousSelected = this.selectedPollOptionValue;
            this.$socket.emit('voteCast', this.roomCode, this.selectedPollOptionValue);
          } else if (this.previousSelected !== this.selectedPollOptionValue) {
            this.$socket.emit('voteChange', this.roomCode, this.previousSelected, this.selectedPollOptionValue);
          }
          this.previousSelected = this.selectedPollOptionValue;
        },

        resetData() {
          this.pollOptions = [];
          this.pollVoteStatus = [];
          this.selectedPollOptionValue = null;
          this.previousSelected = null;
        }

      },
    }
</script>

<style scoped>

  body {
    font: 13px Helvetica, Arial;
    width:100%;
  }

  .center {
    margin: auto;
    width: 25%;
    padding: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .poll {
    margin: auto;
    margin-top: 10px;
    width: 50%;
    padding: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .room-name-chip {
    background-color: #ef5350;
    color:white;
    padding-left:10px;
    padding-right:10px;
  }

  .optDiv {
    display: inline-block;
    float:left;
    width:auto;
    height:auto;
    margin-right: 20px;
    padding-right: 5px;
  }

  .opt-font {
    font-size: medium;
  }

  .vote-percentage {
    font-size: medium;
    width:100px;
  }

  .voteDiv {
    display: inline-block;
    float:right;
    width:100px;
    height:auto;
    margin-left: 5px;
  }

  @media screen and (max-width: 991px) {
    .opt-font {
      font-size: small;
    }

    .vote-percentage {
      width:90px;
      font-size: small;
    }
  }

  @media screen and (max-width: 635px) {
    .opt-font {
      font-size: smaller;
    }

    .vote-percentage {
      width:80px;
      font-size: smaller;
    }
  }

  @media screen and (max-width: 425px) {
    .opt-font {
      font-size: x-small;
    }

    .voteDiv {
      margin-left: 0px;
    }

    .vote-percentage {
      width:60px;
      font-size: x-small;
    }
  }
</style>
