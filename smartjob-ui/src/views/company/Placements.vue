<template>
  <div class="company-placements-page">
    <div class="page-header-container">
      <div class="page-header d-flex justify-content-between align-items-center">
        <div class="header-left">
          <h2 class="fw-bold m-0 gradient-text">Placement Management</h2>
          <p class="text-muted mt-1">Mark candidates as placed for your job postings.</p>
        </div>
      </div>
    </div>

    <!-- MAIN CARD -->
    <div class="premium-table-card">
      <div class="d-flex flex-wrap gap-4 align-items-end mb-4 p-3 rounded-3 bg-light-subtle border border-light">
        <div class="filter-group flex-grow-1" style="max-width: 400px;">
          <label class="form-label text-muted small fw-bold mb-2">Select Job Posting</label>
          <div class="select-wrapper">
            <select v-model="selectedJobId" class="form-select custom-select" @change="fetchApplications">
              <option value="" disabled>--- Select a Job to see Applicants ---</option>
              <option v-for="job in approvedJobs" :key="job.jobId" :value="job.jobId">
                {{ job.title }}
              </option>
            </select>
          </div>
        </div>
        
        <div class="stats-mini d-flex gap-3">
          <div class="mini-stat-badge">
            <span class="small text-muted">Total Applicants:</span>
            <span class="fw-bold ms-1">{{ filteredApplications.length }}</span>
          </div>
        </div>
      </div>

      <div class="table-responsive">
        <table class="table modern-table align-middle">
          <thead>
            <tr>
              <th class="ps-4">Candidate</th>
              <th>Status</th>
              <th>Applied Date</th>
              <th class="text-end pe-4">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="app in filteredApplications" :key="app.applicationId" :class="{ 'placed-row': app.applicationStatus === 'Placed' }">
              <td class="ps-4">
                <div class="d-flex align-items-center">
                  <div class="candidate-avatar me-3">
                    {{ app.fullName.charAt(0) }}
                  </div>
                  <div>
                    <span class="fw-bold text-main d-block">{{ app.fullName }}</span>
                    <span class="text-muted small">{{ app.email }}</span>
                  </div>
                </div>
              </td>
              <td>
                <span class="badge" :class="statusClass(app.applicationStatus)">
                  {{ app.applicationStatus || 'Pending' }}
                </span>
              </td>
              <td>
                <span class="text-muted small">{{ formatDate(app.appliedDate) }}</span>
              </td>
              <td class="text-end pe-4">
                <button 
                  v-if="app.applicationStatus !== 'Placed'"
                  class="btn mark-placed-btn btn-sm"
                  @click="confirmPlacement(app)"
                  :disabled="processingId === app.applicationId"
                >
                  <span v-if="processingId === app.applicationId" class="spinner-border spinner-border-sm me-1"></span>
                  <component v-else :is="CheckCircle" size="14" class="me-1" />
                  Mark as Placed
                </button>
                <span v-else class="text-success fw-bold small d-flex align-items-center justify-content-end gap-1">
                  <component :is="Trophy" size="14" /> Placed
                </span>
              </td>
            </tr>

            <tr v-if="!selectedJobId">
              <td colspan="4" class="text-center py-5 text-muted">
                <div class="empty-state">
                  <component :is="Search" size="48" class="opacity-25 mb-3" />
                  <p>Please select a job from the dropdown to see applicants.</p>
                </div>
              </td>
            </tr>

            <tr v-else-if="filteredApplications.length === 0">
              <td colspan="4" class="text-center py-5 text-muted">
                <div class="empty-state">
                  <component :is="ClipboardList" size="48" class="opacity-25 mb-3" />
                  <p>No applicants found for this job.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- CONFIRM MODAL -->
    <div v-if="showConfirmModal" class="modal-overlay" @click.self="showConfirmModal = false">
      <div class="modal-content placement-modal">
        <div class="modal-header border-0 pb-0">
          <h5 class="fw-bold m-0 p-3">Confirm Choice</h5>
          <button class="btn-close me-2" @click="showConfirmModal = false"></button>
        </div>
        <div class="modal-body text-center py-4">
          <div class="warning-icon mb-3 mx-auto">
            <component :is="AlertTriangle" size="40" class="text-warning" />
          </div>
          <h4 class="fw-bold">Mark {{ targetApp?.fullName }} as Placed?</h4>
          <p class="text-muted px-4">
            This will officially mark the candidate as selected for the <strong>{{ targetJobTitle }}</strong> position. This action will notify the candidate and the central administrator.
          </p>
        </div>
        <div class="modal-footer border-0 pt-0 justify-content-center pb-4">
          <button class="btn btn-light px-4 rounded-pill fw-bold" @click="showConfirmModal = false">Cancel</button>
          <button class="btn btn-primary-gradient px-4 rounded-pill fw-bold" @click="handleMarkPlaced">
            Confirm Placement
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue"
import { getCompanyJobs, getCompanyApplications, markCandidateAsPlaced } from "@/services/api"
import { Search, ClipboardList, CheckCircle, Trophy, AlertTriangle } from "lucide-vue-next"
import { handleError, handleSuccess } from "@/utils/error-handler"


const jobs = ref([])
const allApplications = ref([])
const selectedJobId = ref("")
const processingId = ref(null)
const showConfirmModal = ref(false)
const targetApp = ref(null)

const targetJobTitle = computed(() => {
  const job = jobs.value.find(j => j.jobId === selectedJobId.value)
  return job ? job.title : ""
})

const approvedJobs = computed(() => {
  return jobs.value.filter(j => j.isApproved)
})

const filteredApplications = computed(() => {
  if (!selectedJobId.value) return []
  return allApplications.value.filter(a => a.jobId === selectedJobId.value)
})

const fetchJobs = async () => {
  try {
    jobs.value = await getCompanyJobs()
  } catch (err) {
    handleError(err, "Job Load Error")
  }
}

const fetchApplications = async () => {
  try {
    allApplications.value = await getCompanyApplications()
  } catch (err) {
    handleError(err, "Application Load Error")
  }
}

const statusClass = (status) => {
  if (!status || status === "Pending") return "bg-warning-subtle text-warning"
  if (status === "Placed") return "bg-success-subtle text-success"
  return "bg-secondary-subtle text-secondary"
}

const formatDate = (dateStr) => {
  if (!dateStr) return "N/A"
  return new Date(dateStr).toLocaleDateString()
}

const confirmPlacement = (app) => {
  targetApp.value = app
  showConfirmModal.value = true
}

const handleMarkPlaced = async () => {
  if (!targetApp.value) return
  
  const appId = targetApp.value.applicationId
  processingId.value = appId
  showConfirmModal.value = false
  
  try {
    await markCandidateAsPlaced(appId)
    handleSuccess(`Successfully marked ${targetApp.value.fullName} as Placed!`)
    await fetchApplications() // Refresh list
  } catch (err) {
    handleError(err, "Placement Error")
  } finally {
    processingId.value = null
    targetApp.value = null
  }
}

onMounted(() => {
  fetchJobs()
  fetchApplications()
})
</script>

<style scoped>
.company-placements-page {
  padding-bottom: 50px;
}

.page-header-container {
  background: var(--bg-card);
  padding: 24px 30px;
  border-radius: 20px;
  border: 1px solid var(--border);
  margin-bottom: 30px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.premium-table-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 30px;
  box-shadow: 0 15px 35px -15px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
}

.custom-select {
  border-radius: 12px;
  padding: 12px;
  border: 2px solid var(--border);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.custom-select:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}

.modern-table {
  border-collapse: separate;
  border-spacing: 0 12px;
}

.modern-table tbody tr {
  background: var(--bg-main);
  transition: all 0.3s ease;
  border-radius: 16px;
}

.modern-table tbody tr:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 15px rgba(0,0,0,0.05);
}

.placed-row {
  opacity: 0.8;
  background: #f0fdf4 !important;
}

.candidate-avatar {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #0ea5e9, #2563eb);
  color: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.2rem;
}

.mark-placed-btn {
  background: white;
  border: 1px solid #3b82f6;
  color: #3b82f6;
  font-weight: 700;
  border-radius: 10px;
  padding: 6px 16px;
  transition: all 0.2s;
}

.mark-placed-btn:hover {
  background: #3b82f6;
  color: white;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.3);
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #0ea5e9, #2563eb);
  color: white;
  border: none;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}

.modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(4px);
  display: flex; justify-content: center; align-items: center;
  z-index: 2000;
}

.placement-modal {
  background: white;
  width: 90%;
  max-width: 450px;
  border-radius: 24px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
}

.warning-icon {
  width: 80px;
  height: 80px;
  background: #fffbeb;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
}

.mini-stat-badge {
  background: #f1f5f9;
  padding: 8px 16px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}
</style>
