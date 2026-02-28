<template>
  <div>
    <h2 class="mb-4">Dashboard</h2>

    <!-- Cards Row -->
    <div class="row g-4">
      <!-- Jobs Available -->
      <div class="col-md-4">
        <div class="card shadow-sm hover-card border-primary">
          <div class="card-body">
            <h6 class="text-muted">Jobs Available</h6>
            <h3>{{ stats.totalJobs }}</h3>
          </div>
        </div>
      </div>

      <!-- Applications -->
      <div class="col-md-4">
        <div class="card shadow-sm hover-card border-warning">
          <div class="card-body">
            <h6 class="text-muted">Applications Submitted</h6>
            <h3>{{ stats.appliedJobs }}</h3>
          </div>
        </div>
      </div>

      <!-- Skill Match -->
      <div class="col-md-4">
        <div class="card shadow-sm hover-card border-success">
          <div class="card-body">
            <h6 class="text-muted">Skill Match</h6>
            <h3 class="text-success">{{ stats.skillMatch }}%</h3>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Jobs -->
    <div class="card shadow-sm hover-card mt-4 recent-jobs-card">
      <div class="card-body">
        <h5 class="mb-3">Recent Jobs</h5>

        <table class="table table-hover align-middle">
          <thead>
            <tr>
              <th>Title</th>
              <th>Skills</th>
              <th>Type</th>
              <th>Salary</th>
              <th>Posted</th>
              <th>Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="job in jobs" :key="job.jobId">
              <td>{{ job.title }}</td>
              <td>{{ job.requiredSkills }}</td>
              <td>{{ job.jobType }}</td>
              <td>{{ job.salaryRange }}</td>
              <td>{{ job.postedDate.split("T")[0] }}</td>
              <td>
                <button
                  class="btn btn-sm"
                  :class="job.applied ? 'btn-applied' : 'btn-success'"
                  :disabled="job.applied"
                  @click="apply(job)"
                >
                  {{ job.applied ? "Applied" : "Apply" }}
                </button>
              </td>
            </tr>

            <tr v-if="jobs.length === 0">
              <td colspan="6" class="text-center text-muted">
                No jobs available
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { getJobs, getDashboardData, applyJob } from "../services/api"

/* STATE */
const jobs = ref([])
const stats = ref({
  totalJobs: 0,
  appliedJobs: 0,
  skillMatch: 0
})

/* LOAD DATA USING SESSION */
onMounted(async () => {
  try {
    stats.value = await getDashboardData()
    jobs.value = await getJobs()
  } catch (err) {
    console.error("Dashboard load failed", err)
  }
})

/* APPLY JOB */
async function apply(job) {
  const success = await applyJob(job.jobId)
  if (success) {
    job.applied = true
    stats.value.appliedJobs++
  }
}
</script>

<style scoped>
.hover-card {
  transition: all 0.3s ease;
}

.hover-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 1rem 2rem rgba(0,0,0,0.15);
}

:deep(.table) {
  color: var(--text-main);
}

:deep(.table thead) {
  background-color: var(--recent-bg);
  color: var(--text-main);
}

:deep(.table-hover tbody tr:hover) {
  background-color: rgba(59, 130, 246, 0.15);
}

:deep(.card) {
  background-color: var(--bg-card);
  color: var(--text-main);
}
</style>
