<template>
  <div>
    <h2 class="mb-4">Jobs</h2>

    <div class="row g-4">
      <!-- JOB CARD -->
      <div class="col-md-6" v-for="job in jobs" :key="job.jobId">
        <div class="card shadow-sm hover-card h-100">
          <div class="card-body">
            <h5>{{ job.title }}</h5>
            <p class="text-muted">{{ job.companyName }} • {{ job.location }}</p>

            <p class="small">
              <strong>Skills:</strong> {{ job.requiredSkills }}
            </p>

            <div class="d-flex gap-2 mt-3">
              <button
                class="btn btn-outline-primary btn-sm"
                @click="openInfo(job)"
              >
                Info
              </button>

              <button
                class="btn btn-success btn-sm"
                :disabled="job.applied"
                @click="apply(job)"
              >
                {{ job.applied ? "Applied" : "Apply" }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- INFO MODAL -->
    <div v-if="selectedJob" class="job-modal">
      <div class="job-modal-content">
        <h4>{{ selectedJob.title }}</h4>
        <p><strong>Company:</strong> {{ selectedJob.companyName }}</p>
        <p><strong>Industry:</strong> {{ selectedJob.industry }}</p>
        <p><strong>Location:</strong> {{ selectedJob.location }}</p>
        <p><strong>Type:</strong> {{ selectedJob.jobType }}</p>
        <p><strong>Salary:</strong> {{ selectedJob.salaryRange }}</p>
        <p><strong>Description:</strong> {{ selectedJob.description }}</p>
        <p><strong>Skills:</strong> {{ selectedJob.requiredSkills }}</p>

        <div class="mt-3 d-flex gap-2">
          <button
            class="btn btn-success"
            :disabled="selectedJob.applied"
            @click="apply(selectedJob)"
          >
            {{ selectedJob.applied ? "Applied" : "Apply" }}
          </button>

          <button class="btn btn-secondary" @click="selectedJob = null">
            Close
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { getJobs, applyJob } from "@/services/api"

const jobs = ref([])
const selectedJob = ref(null)

onMounted(async () => {
  jobs.value = await getJobs()
})

function openInfo(job) {
  selectedJob.value = job
}

async function apply(job) {
  const success = await applyJob(job.jobId)
  if (success) {
    job.applied = true
  }
}
</script>

<style scoped>
.hover-card {
  transition: 0.3s ease;
}
.hover-card:hover {
  transform: translateY(-4px);
}

/* MODAL */
.job-modal {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.job-modal-content {
  background: #fff;
  padding: 25px;
  border-radius: 8px;
  max-width: 600px;
  width: 100%;
}
</style>
