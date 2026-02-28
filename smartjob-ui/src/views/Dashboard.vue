<template>
  <div class="user-dashboard-wrapper">
    <div class="dashboard-header mb-5">
      <h2 class="fw-bold m-0 gradient-text">My Dashboard</h2>
      <p class="text-muted mt-1">Welcome back. Here is your recent activity and matches.</p>
    </div>

    <!-- Cards Row -->
    <div class="row g-4 mb-5">
      <!-- Jobs Available -->
      <div class="col-md-4">
        <div class="premium-card glass-blue">
          <div class="card-content">
            <div class="icon-circle bg-blue-subtle">
              <Briefcase class="text-primary" size="24" />
            </div>
            <div class="stats-info">
              <h6 class="text-muted fw-semibold mb-1">Jobs Available</h6>
              <h3 class="fw-bold m-0 text-main">{{ stats.totalJobs }}</h3>
            </div>
          </div>
          <div class="card-glow border-glow-blue"></div>
        </div>
      </div>

      <!-- Applications -->
      <div class="col-md-4">
        <div class="premium-card glass-orange">
          <div class="card-content">
            <div class="icon-circle bg-orange-subtle">
              <FileCheck2 class="text-warning" size="24" />
            </div>
            <div class="stats-info">
              <h6 class="text-muted fw-semibold mb-1">Applications Submitted</h6>
              <h3 class="fw-bold m-0 text-main">{{ stats.appliedJobs }}</h3>
            </div>
          </div>
          <div class="card-glow border-glow-orange"></div>
        </div>
      </div>

      <!-- Skill Match -->
      <div class="col-md-4">
        <div class="premium-card glass-green">
          <div class="card-content">
            <div class="icon-circle bg-green-subtle">
              <TrendingUp class="text-success" size="24" />
            </div>
            <div class="stats-info">
              <h6 class="text-muted fw-semibold mb-1">Skill Match Score</h6>
              <h3 class="fw-bold m-0 text-success">{{ stats.skillMatch }}%</h3>
            </div>
          </div>
          <div class="card-glow border-glow-green"></div>
        </div>
      </div>
    </div>

    <!-- Recent Jobs -->
    <div class="premium-table-card">
      <div class="table-header d-flex justify-content-between align-items-center mb-4">
        <h5 class="fw-bold m-0 text-main">Recommended Jobs</h5>
        <div class="d-flex align-items-center gap-2">
          <button class="btn btn-outline-primary d-flex align-items-center gap-2 rounded-pill px-3 py-1" @click="showFilters = !showFilters">
            <Filter size="16" /> <span class="d-none d-sm-inline">{{ showFilters ? 'Hide Filters' : 'Filters' }}</span>
          </button>
          <span class="badge bg-primary-subtle text-primary fw-semibold px-3 py-2 rounded-pill d-none d-md-block">Latest Postings</span>
        </div>
      </div>

      <!-- Filter Bar -->
      <transition name="fade">
        <div v-if="showFilters" class="filter-bar p-3 mb-4 rounded-3 d-flex flex-wrap gap-3 align-items-center">
        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">City / Location</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.city" placeholder="e.g. New York..." />
          </div>
        </div>
        
        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">Company</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.company" placeholder="e.g. Google..." />
          </div>
        </div>

        <div class="filter-group flex-grow-1">
          <label class="form-label text-muted small fw-semibold mb-1">Salary</label>
          <div class="input-icon-wrapper">
            <input type="text" class="form-control" v-model="filters.salary" placeholder="e.g. 100k..." />
          </div>
        </div>

        <div class="filter-actions mt-3 mt-md-0 d-flex align-items-end">
          <button class="btn btn-outline-secondary d-flex align-items-center gap-2" @click="clearFilters">
            <X size="16" /> Clear Filters
          </button>
        </div>
      </div>
      </transition>

      <div class="table-responsive">
        <table class="table modern-table align-middle">
          <thead>
            <tr>
              <th scope="col" class="ps-4">Job Role</th>
              <th scope="col">Location</th>
              <th scope="col">Requirements</th>
              <th scope="col">Type</th>
              <th scope="col">Salary</th>
              <th scope="col">Posted</th>
              <th scope="col" class="text-end pe-4">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="job in filteredJobs" :key="job.jobId">
              <td class="ps-4">
                <div class="d-flex align-items-center">
                  <div class="job-avatar me-3">
                    <Briefcase size="18" class="text-muted" />
                  </div>
                  <div>
                    <span class="fw-bold text-main d-block">{{ job.title }}</span>
                    <span class="text-muted small">{{ job.companyName || 'SmartJob System' }}</span>
                  </div>
                </div>
              </td>
              <td>
                <div class="d-flex align-items-center gap-1 text-muted">
                  <MapPin size="14" />
                  <span class="fw-medium text-main">{{ job.location || 'Not Specified' }}</span>
                </div>
              </td>
              <td>
                <span class="badge bg-secondary-subtle text-secondary fw-medium rounded-pill">{{ job.requiredSkills }}</span>
              </td>
              <td><span class="text-main fw-medium">{{ job.jobType }}</span></td>
              <td><span class="text-success fw-semibold">{{ job.salaryRange }}</span></td>
              <td><span class="text-muted small">{{ job.postedDate.split("T")[0] }}</span></td>
              <td class="text-end pe-4">
                <button
                  class="btn custom-apply-btn"
                  :class="job.applied ? 'btn-applied' : 'btn-primary-gradient'"
                  :disabled="job.applied"
                  @click="apply(job)"
                >
                  {{ job.applied ? "Applied" : "Apply Now" }}
                </button>
              </td>
            </tr>

            <tr v-if="filteredJobs.length === 0">
              <td colspan="7" class="text-center text-muted py-5">
                <div class="empty-state">
                  <Search size="48" class="text-muted mb-3 opacity-50 mx-auto d-block" />
                  <p class="m-0">No jobs match your filters. Try adjusting your search!</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { getJobs, getDashboardData, applyJob } from "../services/api"
import { Briefcase, FileCheck2, TrendingUp, Search, X, Filter, MapPin } from "lucide-vue-next"

/* STATE */
const jobs = ref([])
const showFilters = ref(false)
const stats = ref({
  totalJobs: 0,
  appliedJobs: 0,
  skillMatch: 0
})

/* FILTERS */
const filters = ref({
  city: "",
  company: "",
  salary: ""
})

/* COMPUTED FILTERED JOBS */
const filteredJobs = computed(() => {
  return jobs.value.filter((job) => {
    // Check City/Location
    const matchCity = !filters.value.city || 
      (job.location && job.location.toLowerCase().includes(filters.value.city.toLowerCase()))
      
    // Check Company
    const matchCompany = !filters.value.company || 
      (job.companyName && job.companyName.toLowerCase().includes(filters.value.company.toLowerCase()))
      
    // Check Salary
    const matchSalary = !filters.value.salary || 
      (job.salaryRange && job.salaryRange.toLowerCase().includes(filters.value.salary.toLowerCase()))

    return matchCity && matchCompany && matchSalary
  })
})

function clearFilters() {
  filters.value.city = ""
  filters.value.company = ""
  filters.value.salary = ""
}

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
.user-dashboard-wrapper {
  padding: 10px;
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
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
}

.premium-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px -10px rgba(0, 0, 0, 0.1);
}

.card-content {
  display: flex;
  align-items: center;
  gap: 20px;
  position: relative;
  z-index: 2;
}

/* Icon Circles */
.icon-circle {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
}

.premium-card:hover .icon-circle {
  transform: scale(1.1) rotate(5deg);
}

.bg-blue-subtle { background: rgba(59, 130, 246, 0.15); }
.bg-orange-subtle { background: rgba(245, 158, 11, 0.15); }
.bg-green-subtle { background: rgba(16, 185, 129, 0.15); }

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
.border-glow-orange { background: linear-gradient(90deg, #f59e0b, #fbbf24); }
.border-glow-green { background: linear-gradient(90deg, #10b981, #34d399); }

/* Text Variables */
.text-main { color: var(--text-main); }

/* Premium Table Layout */
.premium-table-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 30px;
  box-shadow: 0 15px 35px -15px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
}

.modern-table {
  color: var(--text-main);
  border-collapse: separate;
  border-spacing: 0 12px;
}

.modern-table thead th {
  border: none;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 1px;
  padding-bottom: 10px;
}

.modern-table tbody tr {
  background: var(--bg-main);
  transition: all 0.3s ease;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.02);
}

.modern-table tbody tr:hover {
  transform: scale(1.01);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
  background: var(--recent-bg);
}

.modern-table tbody td {
  border: none;
  padding: 16px 12px;
  vertical-align: middle;
}

/* Rounded corners for table rows */
.modern-table tbody td:first-child { border-top-left-radius: 16px; border-bottom-left-radius: 16px; }
.modern-table tbody td:last-child { border-top-right-radius: 16px; border-bottom-right-radius: 16px; }

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

.filter-bar .form-control {
  background: var(--bg-card);
  color: var(--text-main);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 10px 14px;
  transition: all 0.2s;
}

.filter-bar .form-control:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
  outline: none;
}

.filter-bar .form-control::placeholder {
  color: var(--text-muted);
  opacity: 0.6;
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
.custom-apply-btn {
  border-radius: 10px;
  padding: 8px 20px;
  font-weight: 600;
  font-size: 0.875rem;
  transition: all 0.3s ease;
  border: none;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  box-shadow: 0 4px 15px rgba(59, 130, 246, 0.3);
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
  color: white;
}
</style>
