<template>
  <router-view />
  <Notification />
  <ConfirmDialog />
</template>

<script setup>
import Notification from '@/components/Notification.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
</script>