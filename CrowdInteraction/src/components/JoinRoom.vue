<template>
  <div>
    <div style="width: 100%; padding-left: 20px; font-size: medium" class="center title">
      <p>Welcome! Enter the code visible at the bottom right of the game's interface in the box below to join the room, enjoy!</p>
    </div>
    <div class="roomcode-div center">
      <md-field md-inline>
        <label>Room Code</label>
        <md-input v-model="room" class="txtRoomCode" name="room" type="text"></md-input>
      </md-field>
    </div>
    <div class="center">
      <md-button class="md-raised md-accent rounded-button" style="background-color: #ef5350; color: white" type="submit" name="action" v-on:click="joinRoom">Join</md-button>
    </div>
  </div>
</template>

<script>
    export default {
        name: "room",
      data() {
          return {
            room: '',
            displayMsg: '',
            msgColor: ''
          }
      },

      created() {
        //setInterval( () => this.checkConnection(), 5000);
      },

      methods: {
        joinRoom() {
          if (this.room !== '') {
            this.$router.push("/" + this.room);
          } else {
            this.$toasted.error('Veuillez entrer un code de salle.', {
              //theme of the toast you prefer
              theme: '',
              //position of the toast container
              position: 'top-left',
              //display time of the toast
              duration: 2000
            })
          }
          /*if (this.$socket.connected) {
            this.$socket.emit('isRoomValid', this.room, (response) => {
              if (response) {
                this.$router.push("/" + this.room);
              } else {
                this.msgColor = '#FF0000';
                this.displayMsg = "The room you are trying to join does not exist. Make sure you have the correct code."
              }
            });
          }*/
        },

        checkConnection() {
          if (!this.$socket.connected) {
            this.msgColor = '#FF0000';
            this.displayMsg = "Connection was lost, please wait while we try to reconnect you."
          } else {
            this.displayMsg = '';
          }
        },
      }
    }
</script>

<style scoped>

  .roomcode-div{
    width:25%;
    margin: auto;
    margin-bottom: 20px;
  }

  .button-center{
    maragin:auto;
  }

  rounded-button {
    border-radius:50px;
  }

  .center {
    margin: auto;
    width: 25%;
    padding: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .txtRoomCode {
    width: 200px;
  }

  @media screen and (max-width: 991px) {
    .roomcode-div{
      width:40%;
    }

    .title {
      font-size: medium;
    }
  }

  @media screen and (max-width: 635px) {
    .roomcode-div{
      width:50%;
    }

    .title {
      font-size: small;
    }
  }

  @media screen and (max-width: 425px) {
    .roomcode-div{
      width:75%;
    }

    .title {
      font-size: x-small;
    }
  }
</style>
