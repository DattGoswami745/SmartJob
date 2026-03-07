<template>
  <div class="user-dashboard-wrapper">
    <div class="dashboard-header mb-5">
      <div class="d-flex align-items-center justify-content-between mb-2">
        <h2 class="fw-bold m-0 gradient-text">Jobs</h2>
        <div class="d-flex gap-2">
          <!-- Placement Button removed -->
          
          <button class="btn btn-outline-primary d-flex align-items-center gap-2 rounded-pill px-3 py-1" @click="showFilters = !showFilters">
            <Filter size="16" /> <span class="d-none d-sm-inline">{{ showFilters ? 'Hide Filters' : 'Filters' }}</span>
          </button>
        </div>
      </div>
      <p class="text-muted mt-1">Discover your next career opportunity here.</p>
    </div>

    <!-- Filter Bar -->
    <transition name="fade">
      <div v-if="showFilters" class="premium-table-card mb-4">
        <div class="filter-bar p-3 rounded-3 d-flex flex-wrap gap-3 align-items-center mb-0">
        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">Search Role</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.title" placeholder="e.g. Developer..." />
          </div>
        </div>

        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">City / Location</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.location" placeholder="e.g. New York..." />
          </div>
        </div>
        
        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">Company</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.company" placeholder="e.g. Google..." />
          </div>
        </div>

        <div class="filter-actions mt-3 mt-md-0 d-flex align-items-end">
          <button class="btn btn-outline-secondary d-flex align-items-center gap-2" @click="clearFilters">
            <X size="16" /> Clear Filters
          </button>
        </div>
      </div>
      </div>
    </transition>

    <div class="row g-4 mb-5">
      <!-- JOB CARD -->
      <div class="col-md-6 col-lg-4" v-for="job in filteredJobs" :key="job.jobId">
        <div class="premium-card">
          <div class="card-content flex-column align-items-start gap-3 h-100">
            <div class="d-flex align-items-center w-100 mb-2">
              <div class="job-avatar me-3 flex-shrink-0">
                <Briefcase size="20" class="text-primary" />
              </div>
              <div class="flex-grow-1 overflow-hidden">
                <h5 class="fw-bold m-0 text-main text-truncate">{{ job.title }}</h5>
                <span class="text-muted small text-truncate d-block">{{ job.companyName || 'SmartJob System' }}</span>
              </div>
              <div v-if="job.lastDate" class="deadline-badge ms-2 flex-shrink-0" :class="{'urgent': isClosingSoon(job.lastDate)}">
                <Calendar size="12" class="me-1" />
                <span>{{ formatDate(job.lastDate) }}</span>
              </div>
            </div>

            <div class="d-flex align-items-center gap-2 text-muted small">
              <MapPin size="14" />
              <span class="fw-medium text-main">{{ job.location || 'Not Specified' }}</span>
            </div>

            <div class="d-flex align-items-center gap-3 text-muted small">
              <span class="badge bg-secondary-subtle text-secondary fw-medium rounded-pill">{{ job.jobType || 'Full-time' }}</span>
              <span class="text-success fw-semibold">{{ job.salaryRange || 'Competitive' }}</span>
            </div>

            <p class="small text-muted mb-2 line-clamp-2">{{ job.description }}</p>

            <div class="w-100 mt-2 flex-grow-1">
              <p class="small mb-2"><strong class="text-main">Requirements:</strong> <span class="badge bg-primary-subtle text-primary fw-medium rounded-pill">{{ job.requiredSkills }}</span></p>
            </div>

            <div class="d-flex gap-2 w-100 mt-2 pt-3 border-top border-light">
              <button
                class="btn btn-outline-primary flex-grow-1 custom-btn"
                @click="openInfo(job)"
              >
                Details
              </button>

              <button
                class="btn custom-btn flex-grow-1"
                :class="{
                  'btn-applied': job.applied, 
                  'btn-primary-gradient': !job.applied
                }"
                :disabled="job.applied"
                @click="apply(job)"
              >
                {{ job.applied ? "Applied" : "Apply Now" }}
              </button>
            </div>
          </div>
          <div class="card-glow border-glow-blue"></div>
        </div>
      </div>
      
      <!-- Empty State -->
      <div v-if="filteredJobs.length === 0" class="col-12">
        <div class="premium-table-card text-center py-5">
          <div class="empty-state">
            <Search size="48" class="text-muted mb-3 opacity-50 mx-auto d-block" />
            <h5 class="fw-bold text-main mb-2">No jobs match your filters</h5>
            <p class="text-muted m-0">Try adjusting your search criteria!</p>
          </div>
        </div>
      </div>
    </div>

    <!-- INFO MODAL -->
    <transition name="fade">
      <div v-if="selectedJob" class="custom-modal-backdrop" @click.self="selectedJob = null">
        <div class="premium-modal-content premium-table-card">
          <div class="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom border-light">
            <div class="d-flex align-items-center">
              <div class="job-avatar me-3">
                <Briefcase size="24" class="text-primary" />
              </div>
              <div>
                <h4 class="fw-bold m-0 text-main">{{ selectedJob.title }}</h4>
                <span class="text-muted">{{ selectedJob.companyName || 'SmartJob System' }}</span>
              </div>
            </div>
            <button class="btn btn-icon btn-outline-secondary rounded-circle" @click="selectedJob = null">
              <X size="20" />
            </button>
          </div>
          
          <div class="modal-body-content">
            <div class="row g-3 mb-4">
               <div class="col-md-6">
                 <div class="p-3 rounded-3 bg-light-subtle border border-light">
                   <p class="text-muted small mb-1">Industry</p>
                   <p class="fw-medium text-main m-0">{{ selectedJob.industry || 'IT & Tech' }}</p>
                 </div>
               </div>
               <div class="col-md-6">
                 <div class="p-3 rounded-3 bg-light-subtle border border-light">
                   <p class="text-muted small mb-1">Location</p>
                   <div class="d-flex align-items-center gap-1">
                     <MapPin size="16" class="text-muted" />
                     <p class="fw-medium text-main m-0">{{ selectedJob.location || 'Not Specified' }}</p>
                   </div>
                 </div>
               </div>
               <div class="col-md-6">
                 <div class="p-3 rounded-3 bg-light-subtle border border-light">
                   <p class="text-muted small mb-1">Job Type</p>
                   <p class="fw-medium text-main m-0">{{ selectedJob.jobType || 'Full-time' }}</p>
                 </div>
               </div>
               <div class="col-md-6">
                 <div class="p-3 rounded-3 bg-light-subtle border border-light">
                   <p class="text-muted small mb-1">Salary</p>
                   <p class="fw-medium text-success m-0">{{ selectedJob.salaryRange || 'Competitive' }}</p>
                 </div>
               </div>
               <!-- LAST DATE -->
               <div v-if="selectedJob.lastDate" class="col-md-6">
                 <div class="p-3 rounded-3 bg-light-subtle border border-light">
                   <p class="text-muted small mb-1">Last Date to Apply</p>
                   <div class="d-flex align-items-center gap-1">
                     <Calendar size="16" class="text-muted" />
                     <p :class="{'text-danger': isClosingSoon(selectedJob.lastDate)}" class="fw-bold m-0">
                       {{ formatDate(selectedJob.lastDate) }}
                     </p>
                   </div>
                 </div>
               </div>
            </div>

            <div class="mb-4">
              <h6 class="fw-bold text-main mb-2">Job Description</h6>
              <p class="text-muted lh-lg">{{ selectedJob.description || 'No description provided.' }}</p>
            </div>
            
            <div class="mb-4">
              <h6 class="fw-bold text-main mb-2">Required Skills</h6>
              <div class="d-flex flex-wrap gap-2">
                <span class="badge bg-primary-subtle text-primary fw-medium py-2 px-3 rounded-pill">{{ selectedJob.requiredSkills }}</span>
              </div>
            </div>
          </div>

          <div class="mt-4 pt-3 border-top border-light d-flex gap-3 justify-content-end">
            <button class="btn btn-outline-secondary px-4 custom-btn" @click="selectedJob = null">
              Close
            </button>
            <button
              class="btn custom-btn px-5"
              :class="{
                'btn-applied': selectedJob.applied, 
                'btn-primary-gradient': !selectedJob.applied
              }"
              :disabled="selectedJob.applied"
              @click="apply(selectedJob)"
            >
              {{ selectedJob.applied ? "Applied" : "Apply Now" }}
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- Placement Modals Removed -->

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { getJobs, applyJob } from "@/services/api"
import { Briefcase, Search, X, MapPin, Filter, Calendar } from "lucide-vue-next"

const jobs = ref([])
const selectedJob = ref(null)
const showFilters = ref(false)

/* FILTERS */
const filters = ref({
  title: "",
  location: "",
  company: ""
})

onMounted(async () => {
  jobs.value = await getJobs()
})

/* COMPUTED APPLIED JOBS */
const appliedJobs = computed(() => {
  return jobs.value.filter(job => job.applied)
})

/* COMPUTED FILTERED JOBS */
const filteredJobs = computed(() => {
  return jobs.value.filter((job) => {
    const matchTitle = !filters.value.title || 
      (job.title && job.title.toLowerCase().includes(filters.value.title.toLowerCase()))
      
    const matchLocation = !filters.value.location || 
      (job.location && job.location.toLowerCase().includes(filters.value.location.toLowerCase()))
      
    const matchCompany = !filters.value.company || 
      (job.companyName && job.companyName.toLowerCase().includes(filters.value.company.toLowerCase()))

    return matchTitle && matchLocation && matchCompany
  })
})

function clearFilters() {
  filters.value.title = ""
  filters.value.location = ""
  filters.value.company = ""
}

function openInfo(job) {
  selectedJob.value = job
}

async function apply(job) {
  const success = await applyJob(job.jobId)
  if (success) {
    job.applied = true
    if (selectedJob.value && selectedJob.value.jobId === job.jobId) {
       selectedJob.value.applied = true
    }
  }
}

function formatDate(dateStr) {
  if (!dateStr) return null
  return dateStr.split("T")[0]
}

function isClosingSoon(dateStr) {
  if (!dateStr) return false
  const deadline = new Date(dateStr)
  const now = new Date()
  const diffTime = deadline - now
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays <= 3 && diffDays >= 0
}
</script>

<style scoped>
.user-dashboard-wrapper {
  padding: 0;
}

/* Gradient Text for Header */
.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* Base Premium Card */
.premium-card {
  position: relative;
  background: var(--bg-card);
  border-radius: 20px;
  padding: 24px;
  overflow: hidden;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
  z-index: 1;
  height: 100%;
}

.premium-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px -10px rgba(0, 0, 0, 0.1);
}

.card-content {
  display: flex;
  position: relative;
  z-index: 2;
  height: 100%;
}

/* Glow Effects */
.card-glow {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 4px;
  opacity: 0.7;
  transition: height 0.3s ease;
}

.premium-card:hover .card-glow {
  height: 100%;
  opacity: 0.05;
}

.border-glow-blue { background: linear-gradient(90deg, #3b82f6, #60a5fa); }

/* Premium Table/Block Layout (reused from Dashboard) */
.premium-table-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 30px;
  box-shadow: 0 15px 35px -15px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
}

/* Filter Bar */
.filter-bar {
  background: var(--bg-main);
  border: 1px solid var(--border);
}

.filter-bar .form-label {
  letter-spacing: 0.5px;
  text-transform: uppercase;
  font-size: 0.70rem;
}

.filter-bar .form-control, .premium-input {
  background: var(--bg-card);
  color: var(--text-main);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 10px 14px;
  transition: all 0.2s;
}

.filter-bar .form-control:focus, .premium-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
  outline: none;
}

.filter-bar .form-control::placeholder {
  color: var(--text-muted);
  opacity: 0.6;
}

/* Deadline Badge */
.deadline-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 8px;
  background: var(--recent-bg);
  border: 1px solid var(--border);
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 600;
  white-space: nowrap;
}

.deadline-badge.urgent {
  background: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.2);
  color: #ef4444;
  animation: pulse-red 2s infinite;
}

@keyframes pulse-red {
  0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4); }
  70% { box-shadow: 0 0 0 6px rgba(239, 68, 68, 0); }
  100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
}

/* Job Avatar Box */
.job-avatar {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--recent-bg);
  border: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Custom Buttons */
.custom-btn {
  border-radius: 10px;
  padding: 8px 20px;
  font-weight: 600;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  box-shadow: 0 4px 15px rgba(59, 130, 246, 0.3);
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
  color: white;
}

/* Text Variables */
.text-main { color: var(--text-main); }
.border-light { border-color: var(--border) !important; }
.bg-light-subtle { background-color: var(--recent-bg) !important; }

/* Utilities */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* CUSTOM MODAL */
.custom-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(17, 24, 39, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  padding: 20px;
}

.premium-modal-content {
  width: 100%;
  max-width: 700px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 0 !important;
}

.premium-modal-content > div:first-child {
  padding: 24px 30px;
  background: var(--bg-card);
  z-index: 10;
}

.modal-body-content {
  padding: 24px 30px;
  overflow-y: auto;
  background: var(--bg-card);
}

.premium-modal-content > div:last-child {
  padding: 20px 30px;
  background: var(--bg-main);
  border-top: 1px solid var(--border);
}

.btn-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
.fade-enter-active .premium-modal-content {
  transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.fade-enter-from .premium-modal-content {
  transform: scale(0.95) translateY(10px);
}

/* Success Icon Animation */
.success-icon-container {
  width: 80px;
  height: 80px;
  background: rgba(16, 185, 129, 0.15);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: scale-up 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

@keyframes scale-up {
  0% { transform: scale(0); opacity: 0; }
  100% { transform: scale(1); opacity: 1; }
}
</style>
