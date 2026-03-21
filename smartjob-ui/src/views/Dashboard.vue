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
              <th scope="col">Last Date</th>
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
                <div class="d-flex flex-wrap gap-1">
                  <span 
                    v-for="(skill, index) in (job.requiredSkills || '').split(',')" 
                    :key="index"
                    class="badge rounded-pill skill-tag"
                    :class="'tag-color-' + (index % 5)"
                  >
                    {{ skill.trim() }}
                  </span>
                </div>
              </td>
              <td><span class="text-main fw-medium">{{ job.jobType }}</span></td>
              <td><span class="text-success fw-semibold">{{ job.salaryRange }}</span></td>
              <td>
                <span :class="{'text-danger': isClosingSoon(job.lastDate), 'text-muted': !isClosingSoon(job.lastDate)}" class="small fw-semibold">
                  {{ formatDate(job.lastDate) || 'No Deadline' }}
                </span>
              </td>
              <td><span class="text-muted small">{{ job.postedDate.split("T")[0] }}</span></td>
              <td class="text-end pe-4 d-flex gap-2 justify-content-end">
                <button 
                  class="btn btn-outline-primary btn-sm rounded-pill px-3"
                  @click="openInfo(job)"
                >
                  Details
                </button>
                <button
                  class="btn custom-apply-btn"
                  :class="{
                    'btn-applied': job.applied && !isUserPlaced && job.applicationStatus !== 'Placed', 
                    'btn-success': job.applicationStatus === 'Placed',
                    'btn-disabled-faded': isUserPlaced && job.applicationStatus !== 'Placed',
                    'btn-primary-gradient': !job.applied && !isUserPlaced
                  }"
                  :disabled="job.applied || isUserPlaced"
                  @click="apply(job)"
                >
                  {{ 
                    job.applicationStatus === 'Placed' ? "🎉 Placed!" : 
                    (isUserPlaced ? "Cannot Apply" : 
                    (job.applied ? "Applied" : "Apply Now")) 
                  }}
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

    <!-- PROFILE COMPLETION POPUP -->
    <div v-if="showProfileReminder" class="profile-reminder-overlay">
      <div class="profile-reminder-content">
        <div class="text-center mb-4">
          <div class="icon-box-large pulse-animation mx-auto mb-3">
            <UserIcon class="text-primary" size="48" />
          </div>
          <h3 class="fw-bold gradient-text">Complete Your Profile!</h3>
          <p class="text-muted">You're almost there! Complete your profile to get better job matches and stand out to recruiters.</p>
        </div>

        <div class="reminder-features mb-4">
          <div class="feature-item">
            <div class="feature-icon"><Zap size="18" /></div>
            <span>Boost your skill match score</span>
          </div>
          <div class="feature-item">
            <div class="feature-icon"><Target size="18" /></div>
            <span>Get personalized recommendations</span>
          </div>
          <div class="feature-item">
            <div class="feature-icon"><ShieldCheck size="18" /></div>
            <span>Build trust with companies</span>
          </div>
        </div>

        <div class="d-grid gap-3">
          <button @click="goToProfile" class="btn btn-primary-gradient py-3 fw-bold">
            Complete My Profile <ArrowRight size="20" class="ms-2" />
          </button>
          <button @click="dismissReminder" class="btn btn-link text-muted text-decoration-none small">
            Remind me later
          </button>
        </div>
      </div>
    </div>

    <!-- CONGRATULATIONS POPUP -->
    <div v-if="showCongratulations" class="profile-reminder-overlay">
      <div class="profile-reminder-content text-center congratulation-card">
        <div class="confetti-container">
          <div class="congs-icon mx-auto mb-3 pulse-animation">
            <Trophy class="text-warning" size="64" />
          </div>
        </div>
        <h2 class="fw-bold gradient-text mb-2">Congratulations!</h2>
        <h4 class="fw-bold text-main mb-3">You've Been Placed!</h4>
        <p class="text-muted mb-4 px-3">
          Awesome news! You have been officially selected for the <strong>{{ placedJobTitle }}</strong> position. 
          The company will reach out to you soon for the next steps.
        </p>

        <div class="success-features mb-4 p-3 bg-light rounded-4">
          <div class="d-flex align-items-center gap-2 justify-content-center text-success fw-bold">
            <Zap size="20" /> Start your new career journey today!
          </div>
        </div>

        <button @click="closeCongratulations" class="btn btn-primary-gradient w-100 py-3 rounded-pill fw-bold shadow-sm">
          Got it, Thanks!
        </button>
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
              
              <!-- Case 1: Uploaded File -->
              <div v-if="selectedJob.jobDescriptionFile" class="file-view-box p-3 rounded-3 border border-primary-subtle bg-primary-subtle bg-opacity-10 d-flex align-items-center mb-3">
                <i class="bi bi-file-earmark-pdf-fill text-danger fs-3 me-3" v-if="selectedJob.jobDescriptionFile.endsWith('.pdf')"></i>
                <i class="bi bi-file-earmark-word-fill text-primary fs-3 me-3" v-else></i>
                <div class="flex-grow-1">
                  <p class="mb-0 fw-bold text-main small">Official Description Document</p>
                  <p class="mb-0 text-muted small">{{ selectedJob.jobDescriptionFile.split('/').pop() }}</p>
                </div>
                <a :href="API_HOST + selectedJob.jobDescriptionFile" target="_blank" class="btn btn-sm btn-primary rounded-pill px-3">
                  <i class="bi bi-download me-1"></i> View/Download
                </a>
              </div>

              <!-- Case 2: Rich Text -->
              <div v-if="selectedJob.jobDescriptionText" class="rich-text-view p-3 rounded-3 bg-light border border-light mb-3">
                <div class="description-content" style="white-space: pre-line;">{{ selectedJob.jobDescriptionText }}</div>
              </div>

              <!-- Fallback: Basic Description -->
              <p v-if="!selectedJob.jobDescriptionFile && !selectedJob.jobDescriptionText" class="text-muted lh-lg">{{ selectedJob.description || 'No description provided.' }}</p>
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
                'btn-applied': selectedJob.applied || isUserPlaced, 
                'btn-primary-gradient': !selectedJob.applied && !isUserPlaced
              }"
              :disabled="selectedJob.applied || isUserPlaced"
              @click="apply(selectedJob)"
            >
              {{ 
                selectedJob.applicationStatus === 'Placed' ? "🎉 Placed!" : 
                (isUserPlaced ? "Cannot Apply" : 
                (selectedJob.applied ? "Applied" : "Apply Now")) 
              }}
            </button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { getJobs, getDashboardData, applyJob, API_HOST } from "@/services/api"
import { Briefcase, FileCheck2, TrendingUp, Search, X, Filter, MapPin, User as UserIcon, ArrowRight, Zap, Target, ShieldCheck, Trophy } from "lucide-vue-next"
import { useRouter } from "vue-router"

const router = useRouter()

/* STATE */
const jobs = ref([])
const showFilters = ref(false)
const stats = ref({
  totalJobs: 0,
  appliedJobs: 0,
  skillMatch: 0,
  isProfileComplete: true
})

const showProfileReminder = ref(false)
const showCongratulations = ref(false)
const placedJobTitle = ref("")

/* FILTERS */
const filters = ref({
  city: "",
  company: "",
  salary: ""
})

/* COMPUTED IS PLACED */
const isUserPlaced = computed(() => {
  return stats.value.isPlaced === true
})

/* COMPUTED FILTERED JOBS */
const filteredJobs = computed(() => {
  if (!Array.isArray(jobs.value)) return []
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

function openInfo(job) {
  selectedJob.value = job
}

/* LOAD DATA USING SESSION */
onMounted(async () => {
  try {
    const data = await getDashboardData()
    stats.value = data
    const jobsRes = await getJobs()
    jobs.value = (Array.isArray(jobsRes) ? jobsRes : jobsRes.jobs) || []

    // 🏆 Check for Placement Success
    const isPlaced = data.isPlaced || false
    if (isPlaced) {
      // Find the placed job in the list to getting its title
      const placedJob = jobs.value.find(j => j.applicationStatus === 'Placed')
      if (placedJob) {
        placedJobTitle.value = placedJob.title
        const hasShown = localStorage.getItem(`congs_shown_${placedJob.jobId}`)
        if (!hasShown) {
          setTimeout(() => {
            showCongratulations.value = true
          }, 1500)
        }
      }
    }

    // Show reminder if profile is incomplete
    if (!data.isProfileComplete && !placedJob) {
      setTimeout(() => {
        showProfileReminder.value = true
      }, 1000)
    }
  } catch (err) {
    console.error("Dashboard load failed", err)
  }
})

function goToProfile() {
  showProfileReminder.value = false
  router.push("/app/profile")
}

function dismissReminder() {
  showProfileReminder.value = false
}

function closeCongratulations() {
  showCongratulations.value = false
  const placedJob = jobs.value.find(j => j.applicationStatus === 'Placed')
  if (placedJob) {
    localStorage.setItem(`congs_shown_${placedJob.jobId}`, "true")
  }
}

/* APPLY JOB */
async function apply(job) {
  const success = await applyJob(job.jobId)
  if (success) {
    job.applied = true
    stats.value.appliedJobs++
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

@media (max-width: 575.98px) {
  .dashboard-header h2 {
    font-size: 1.5rem;
  }
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
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: none;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #0ea5e9, #2563eb);
  color: white !important;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(37, 99, 235, 0.3);
  filter: brightness(1.1);
}

.btn-applied {
  background: #dcfce7 !important;
  color: #15803d !important;
  border: 1px solid #bbf7d0 !important;
  opacity: 1 !important;
  cursor: default;
}

.btn-disabled-faded {
  background: #dcfce7 !important; /* Light Green background */
  color: #10b981 !important; /* Green text */
  border: 1px solid #a7f3d0 !important;
  opacity: 0.6 !important;
  cursor: not-allowed !important; /* Stop sign cursor */
}

.btn-success {
  background: #22c55e !important;
  color: white !important;
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
}

.btn-disabled-faded {
  background: #dcfce7 !important; /* Light Green background */
  color: #10b981 !important; /* Green text */
  border: 1px solid #a7f3d0 !important;
  opacity: 0.6 !important;
  cursor: not-allowed !important; /* Stop sign cursor */
}

.btn-success {
  background: #22c55e !important;
  color: white !important;
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
}

/* Profile Reminder Overlay */
.profile-reminder-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.profile-reminder-content {
  background: white;
  max-width: 480px;
  width: 100%;
  border-radius: 30px;
  padding: 40px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: modalSlideUp 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

@keyframes modalSlideUp {
  from { transform: translateY(40px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.icon-box-large {
  width: 100px;
  height: 100px;
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.1), rgba(139, 92, 246, 0.1));
  border-radius: 25px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pulse-animation {
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(1); box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.4); }
  70% { transform: scale(1.05); box-shadow: 0 0 0 15px rgba(59, 130, 246, 0); }
  100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(59, 130, 246, 0); }
}

.reminder-features {
  background: #f8fafc;
  padding: 20px;
  border-radius: 20px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
  color: #475569;
  font-size: 0.9rem;
  font-weight: 500;
}

.feature-item:last-child { margin-bottom: 0; }

.feature-icon {
  width: 28px;
  height: 28px;
  background: white;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #3b82f6;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

/* Colorful Skill Tags */
.skill-tag {
  padding: 4px 10px;
  font-weight: 600;
  font-size: 0.7rem;
  border: 1px solid transparent;
}
.tag-color-0 { background: rgba(59, 130, 246, 0.1); color: #2563eb; border-color: rgba(59, 130, 246, 0.2); }
.tag-color-1 { background: rgba(16, 185, 129, 0.1); color: #059669; border-color: rgba(16, 185, 129, 0.2); }
.tag-color-2 { background: rgba(245, 158, 11, 0.1); color: #d97706; border-color: rgba(245, 158, 11, 0.2); }
.tag-color-3 { background: rgba(139, 92, 246, 0.1); color: #7c3aed; border-color: rgba(139, 92, 246, 0.2); }
.tag-color-4 { background: rgba(236, 72, 153, 0.1); color: #db2777; border-color: rgba(236, 72, 153, 0.2); }
</style>
