import Vue from 'vue'
import Router from 'vue-router'
import JoinRoom from '@/components/JoinRoom'
import Room from '@/components/Room'

Vue.use(Router)

export default new Router({
  mode: 'history',
  routes: [
    {
      path: '/',
      name: 'JoinRoom',
      component: JoinRoom
    },
    {
      path: '/:roomcode',
      name: 'Room',
      component: Room
    }
  ]
})
