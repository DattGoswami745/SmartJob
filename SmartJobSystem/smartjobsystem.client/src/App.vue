<template>
  <Login v-if="!loggedIn" />
  <Dashboard v-else />
</template>

<script setup>
  import { ref } from 'vue'
  import Login from './pages/Login.vue'
  import Dashboard from './pages/Dashboard.vue'

  const loggedIn = ref(false)
</script>
