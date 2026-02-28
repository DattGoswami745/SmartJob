<template>
  <router-link
    :to="to"
    class="flex items-center gap-3 px-4 py-2 rounded-lg transition
           hover:bg-slate-800"
    :class="highlight ? 'text-green-400' : ''"
  >
    <component :is="iconComponent" class="w-5 h-5" />
    <span class="font-medium">{{ text }}</span>
  </router-link>
</template>

<script setup>
import * as icons from "lucide-vue-next"
import { computed } from "vue"

const props = defineProps({
  icon: String,
  text: String,
  to: String,
  highlight: Boolean
})

const iconComponent = computed(() => icons[props.icon])
</script>
