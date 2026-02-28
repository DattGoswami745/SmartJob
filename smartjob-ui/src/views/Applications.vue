<template>
  <div>
    <h2 class="mb-4">My Applications</h2>

    <div class="row g-4">
      <div
        v-for="app in applications"
        :key="app.applicationId"
        class="col-md-6"
      >
        <div class="card shadow-sm hover-card">
          <div class="card-body">
            <h5>{{ app.title }}</h5>
            <p class="text-muted">{{ app.company }}</p>

            <span
              class="badge"
              :class="statusClass(app.status)"
            >
              {{ app.status }}
            </span>
          </div>
        </div>
      </div>

      <div
        v-if="applications.length === 0"
        class="col-12 text-center text-muted"
      >
        You haven’t applied to any jobs yet
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { getMyApplications } from "../services/api"

const applications = ref([])

onMounted(async () => {
  try {
    applications.value = await getMyApplications()
  } catch (err) {
    console.error("Failed to load applications", err)
  }
})

function statusClass(status) {
  if (status === "Applied" || status === "Pending") return "bg-warning"
  if (status === "Shortlisted") return "bg-success"
  if (status === "Rejected") return "bg-danger"
  return "bg-secondary"
}
</script>

<style scoped>
.hover-card {
  transition: 0.3s ease;
}
.hover-card:hover {
  transform: translateY(-4px);
}
</style>
