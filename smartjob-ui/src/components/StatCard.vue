<template>
  <div
    class="bg-white rounded-xl p-6 shadow-sm border"
    :class="highlight ? 'border-green-400' : ''"
  >
    <p class="text-slate-500">{{ title }}</p>
    <h3 class="text-3xl font-bold text-slate-800 mt-2">
      {{ value }}
    </h3>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  value: String,
  highlight: Boolean
})
</script>
