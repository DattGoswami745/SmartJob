<template>
  <div class="d-flex min-vh-100">
    <Sidebar />

    <div class="flex-grow-1 p-4 content-area" style="margin-left: 240px; overflow-x: hidden;">
      <router-view />
    </div>
  </div>
</template>

<script setup>
import Sidebar from "@/components/Sidebar.vue";
import { useRouter } from "vue-router";

const router = useRouter();


</script>
