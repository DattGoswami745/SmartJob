<template>
  <div class="jobs-page">

    <!-- HEADER & FILTERS -->
    <div class="page-header-container">
      <div class="page-header">
        <div class="header-left">
          <h2>Central Jobs</h2>
        </div>
      </div>

      <div class="filters-bar">
        <div class="search-box">
          <i class="bi bi-search"></i>
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="Search by title..." 
            class="filter-input"
          />
        </div>

        <div class="filter-group">
          <i class="bi bi-building"></i>
          <select v-model="selectedCompany" class="filter-select">
            <option value="">All Companies</option>
            <option v-for="c in companies" :key="c.companyId" :value="c.companyId">
              {{ c.companyName }}
            </option>
          </select>
        </div>

        <div class="filter-group">
          <i class="bi bi-briefcase"></i>
          <select v-model="selectedJobType" class="filter-select">
            <option value="">All Types</option>
            <option v-for="type in ['Full-time', 'Part-time', 'Contract', 'Internship', 'Remote']" :key="type" :value="type">
              {{ type }}
            </option>
          </select>
        </div>

        <button class="reset-btn" @click="resetFilters" title="Reset Filters">
          <i class="bi bi-arrow-counterclockwise"></i>
        </button>
      </div>
    </div>

    <!-- SECTIONS -->
    <div v-if="loading && jobs.length === 0" class="text-center py-5">
      <div class="spinner-border text-primary" role="status"></div>
      <p class="mt-2 text-muted">Loading all jobs...</p>
    </div>

    <div v-else class="sections-container">
      
      <!-- PENDING SECTION -->
      <section class="job-section">
        <h3 class="section-title pending-title">
          <i class="bi bi-clock-history"></i> Pending Approvals 
          <span class="count-badge">{{ pendingJobs.length }}</span>
        </h3>
        <div v-if="pendingJobs.length > 0" class="jobs-grid">
          <div v-for="job in pendingJobs" :key="job.jobId" class="job-card pending-border">
            <div class="card-header-top">
              <span class="company-badge">{{ companies.find(c => c.companyId === job.companyId)?.companyName || 'Company' }}</span>
              <span class="status-badge pending">Pending</span>
            </div>
            <h3 class="job-title">{{ job.title }}</h3>
            <div class="job-meta">
              <div class="meta-item"><i class="bi bi-briefcase"></i> {{ job.jobType }}</div>
              <div class="meta-item"><i class="bi bi-cash-stack"></i> {{ job.salaryRange }}</div>
              <div class="meta-item"><i class="bi bi-calendar3"></i> {{ job.postedDate ? job.postedDate.split('T')[0] : 'N/A' }}</div>
            </div>
            <div class="card-footer">
              <button class="detail-btn" @click="showDetails(job)">Details</button>
              <button class="approve-card-btn" @click="handleApprove(job.jobId)">Approve</button>
              <button class="reject-card-btn" @click="handleReject(job.jobId)">Reject</button>
            </div>
          </div>
        </div>
        <div v-else class="no-data-inline">No pending requests found.</div>
      </section>

      <!-- APPROVED SECTION -->
      <section class="job-section">
        <h3 class="section-title approved-title collapsible-header" @click="toggleApproved">
          <div class="title-left">
            <i class="bi bi-check-circle"></i> Approved Jobs
            <span class="count-badge">{{ approvedJobs.length }}</span>
          </div>
          <i :class="['bi', isApprovedCollapsed ? 'bi-chevron-down' : 'bi-chevron-up', 'collapse-arrow']"></i>
        </h3>
        <div v-show="!isApprovedCollapsed">
          <div v-if="approvedJobs.length > 0" class="jobs-grid">
            <div v-for="job in approvedJobs" :key="job.jobId" class="job-card approved-border">
              <div class="card-header-top">
                <span class="company-badge">{{ companies.find(c => c.companyId === job.companyId)?.companyName || 'Company' }}</span>
                <span class="status-badge approved">Approved</span>
              </div>
              <h3 class="job-title">{{ job.title }}</h3>
              <div class="job-meta">
                <div class="meta-item"><i class="bi bi-briefcase"></i> {{ job.jobType }}</div>
                <div class="meta-item"><i class="bi bi-calendar3"></i> {{ job.postedDate ? job.postedDate.split('T')[0] : 'N/A' }}</div>
              </div>
              <div class="card-footer">
                <button class="detail-btn" @click="showDetails(job)">Details</button>
              </div>
            </div>
          </div>
          <div v-else class="no-data-inline">No approved jobs found.</div>
        </div>
      </section>

      <!-- REJECTED SECTION -->
      <section class="job-section">
        <h3 class="section-title rejected-title collapsible-header" @click="toggleRejected">
          <div class="title-left">
            <i class="bi bi-x-circle"></i> Rejected Jobs
            <span class="count-badge">{{ rejectedJobs.length }}</span>
          </div>
          <i :class="['bi', isRejectedCollapsed ? 'bi-chevron-down' : 'bi-chevron-up', 'collapse-arrow']"></i>
        </h3>
        <div v-show="!isRejectedCollapsed">
          <div v-if="rejectedJobs.length > 0" class="jobs-grid">
            <div v-for="job in rejectedJobs" :key="job.jobId" class="job-card rejected-border">
              <div class="card-header-top">
                <span class="company-badge">{{ companies.find(c => c.companyId === job.companyId)?.companyName || 'Company' }}</span>
                <span class="status-badge rejected">Rejected</span>
              </div>
              <h3 class="job-title">{{ job.title }}</h3>
              <div class="job-meta">
                <div class="meta-item"><i class="bi bi-briefcase"></i> {{ job.jobType }}</div>
              </div>
              <div class="card-footer">
                <button class="detail-btn" @click="showDetails(job)">Details</button>
                <button class="restore-card-btn" @click="handleRestore(job.jobId)">Restore</button>
              </div>
            </div>
          </div>
          <div v-else class="no-data-inline">No rejected jobs found.</div>
        </div>
      </section>

    </div>

    <!-- JOB DETAILS MODAL -->
    <div v-if="selectedJob" class="modal-overlay" @click.self="selectedJob = null">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Job Details</h2>
          <button class="close-modal-btn" @click="selectedJob = null">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="detail-group">
            <label>Title</label>
            <p class="detail-value">{{ selectedJob.title }}</p>
          </div>
          <div class="detail-group">
            <label>Company</label>
            <p class="detail-value">{{ companies.find(c => c.companyId === selectedJob.companyId)?.companyName || 'Unknown' }}</p>
          </div>
          <div class="detail-row">
            <div class="detail-group">
              <label>Type</label>
              <p class="detail-value">{{ selectedJob.jobType }}</p>
            </div>
            <div class="detail-group">
              <label>Salary</label>
              <p class="detail-value">{{ selectedJob.salaryRange }}</p>
            </div>
          </div>
          <div class="detail-group">
            <label>Job Description</label>
            
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
            <div v-if="!selectedJob.jobDescriptionFile && !selectedJob.jobDescriptionText" class="description-text">{{ selectedJob.description }}</div>
          </div>
          <div class="detail-group">
            <label>Required Skills</label>
            <div class="skills-list">
              <span 
                v-for="(skill, index) in (selectedJob.requiredSkills || '').split(',')" 
                :key="index"
                class="badge rounded-pill skill-tag"
                :class="'tag-color-' + (index % 5)"
              >
                {{ skill.trim() }}
              </span>
            </div>
          </div>
          <div class="detail-group">
            <label>Posted On</label>
            <p class="detail-value">{{ selectedJob.postedDate?.split('T')[0] }}</p>
          </div>
        </div>
        <div class="modal-footer">
          <button v-if="!selectedJob.isApproved" class="primary-btn" @click="approveFromModal(selectedJob.jobId)">
            <i class="bi bi-check2-circle me-1"></i> Approve Job
          </button>
          <button v-if="!selectedJob.isApproved && selectedJob.isActive" class="danger-btn" @click="rejectFromModal(selectedJob.jobId)">
            <i class="bi bi-x-circle me-1"></i> Reject Job
          </button>
          <button v-if="!selectedJob.isActive" class="primary-btn" @click="restoreFromModal(selectedJob.jobId)">
            <i class="bi bi-arrow-counterclockwise me-1"></i> Restore Job
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import { ref, onMounted, computed, watch } from "vue"
import {
   getCentralJobs,
   approveJob,
   rejectJob,
   restoreJob,
   getCentralCompanies,
   API_HOST
 } from "@/services/api"
import { useNotification } from "@/composables/useNotification"
import { useConfirm } from "@/composables/useConfirm"

export default {
  setup() {
    const { notify } = useNotification()
    const { confirm } = useConfirm()

    const jobs = ref([])
    const companies = ref([])
    const loading = ref(false)

    const selectedJob = ref(null)
    const searchQuery = ref("")
    const selectedCompany = ref("")
    const selectedJobType = ref("")
    const isApprovedCollapsed = ref(true)
    const isRejectedCollapsed = ref(true)

    const loadJobs = async () => {
      try {
        loading.value = true
        jobs.value = await getCentralJobs("All")
      } catch (err) {
        notify("Error loading jobs: " + err.message, "error")
      } finally {
        loading.value = false
      }
    }

    const loadCompanies = async () => {
      try {
        companies.value = await getCentralCompanies()
      } catch (err) {
        notify("Error loading companies: " + err.message, "error")
      }
    }

    const handleApprove = async (jobId) => {
      if (!(await confirm("Are you sure you want to approve this job?", "Approve Job"))) return
      try {
        loading.value = true
        await approveJob(jobId)
        notify("Job Approved Successfully", "success")
        await loadJobs()
      } catch (err) {
        notify("Approval failed: " + err.message, "error")
      } finally {
        loading.value = false
      }
    }

    const handleReject = async (jobId) => {
      if (!(await confirm("Are you sure you want to reject this job? It will be permanently hidden.", "Reject Job"))) return
      try {
        loading.value = true
        await rejectJob(jobId)
        notify("Job Rejected Successfully", "success")
        await loadJobs()
      } catch (err) {
        notify("Rejection failed: " + err.message, "error")
      } finally {
        loading.value = false
      }
    }

    const handleRestore = async (jobId) => {
      if (!(await confirm("Are you sure you want to restore this job?", "Restore Job"))) return
      try {
        loading.value = true
        await restoreJob(jobId)
        notify("Job Restored Successfully", "success")
        await loadJobs()
      } catch (err) {
        notify("Restoration failed: " + err.message, "error")
      } finally {
        loading.value = false
      }
    }

    const showDetails = (job) => {
      selectedJob.value = job
    }

    const approveFromModal = async (jobId) => {
      await handleApprove(jobId)
      selectedJob.value = null
    }

    const rejectFromModal = async (jobId) => {
      await handleReject(jobId)
      selectedJob.value = null
    }

    const restoreFromModal = async (jobId) => {
      await handleRestore(jobId)
      selectedJob.value = null
    }

    const pendingJobs = computed(() => {
      return filteredJobs.value.filter(j => !j.isApproved && j.isActive)
    })

    const approvedJobs = computed(() => {
      return filteredJobs.value.filter(j => j.isApproved && j.isActive)
    })

    const rejectedJobs = computed(() => {
      return filteredJobs.value.filter(j => !j.isActive)
    })

    const toggleApproved = () => {
      isApprovedCollapsed.value = !isApprovedCollapsed.value
    }

    const toggleRejected = () => {
      isRejectedCollapsed.value = !isRejectedCollapsed.value
    }

    const filteredJobs = computed(() => {
      let filtered = jobs.value
      
      const q = searchQuery.value.toLowerCase()
      if (q) {
        filtered = filtered.filter(j => j.title?.toLowerCase().includes(q))
      }

      if (selectedCompany.value) {
        filtered = filtered.filter(j => j.companyId === parseInt(selectedCompany.value))
      }

      if (selectedJobType.value) {
        filtered = filtered.filter(j => j.jobType === selectedJobType.value)
      }

      return filtered
    })

    const resetFilters = () => {
      searchQuery.value = ""
      selectedCompany.value = ""
      selectedJobType.value = ""
    }

    onMounted(() => {
      loadJobs()
      loadCompanies()
    })

     return {
       jobs,
       filteredJobs,
       pendingJobs,
       approvedJobs,
       rejectedJobs,
       companies,
        loading,
        searchQuery,
        handleApprove,
        handleReject,
        handleRestore,
        selectedJob,
        showDetails,
        approveFromModal,
        rejectFromModal,
        restoreFromModal,
        isApprovedCollapsed,
        isRejectedCollapsed,
        toggleApproved,
        toggleRejected,
        selectedCompany,
        selectedJobType,
        resetFilters
     }
  }
}
</script>

<style scoped>
/* Header & Filters */
.page-header-container {
  background: var(--bg-card);
  padding: 24px;
  border-radius: 20px;
  border: 1px solid var(--border);
  margin-bottom: 40px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.page-header {
  margin-bottom: 24px;
}

.filters-bar {
  display: flex;
  gap: 16px;
  align-items: center;
  flex-wrap: wrap;
}

.search-box {
  position: relative;
  flex: 2;
  min-width: 250px;
}

.search-box i {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
}

.filter-input {
  width: 100%;
  padding: 10px 15px 10px 40px;
  border-radius: 12px;
  border: 1px solid var(--border);
  background: var(--bg-main);
  font-size: 14px;
  transition: 0.2s;
}

.filter-group {
  position: relative;
  flex: 1;
  min-width: 180px;
}

.filter-group i {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #64748b;
  pointer-events: none;
}

.filter-select {
  width: 100%;
  padding: 10px 15px 10px 38px;
  border-radius: 12px;
  border: 1px solid var(--border);
  background: var(--bg-main);
  font-size: 14px;
  color: var(--text-primary);
  appearance: none;
  cursor: pointer;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%2394a3b8' class='bi bi-chevron-down' viewBox='0 0 16 16'%3E%3Cpath fill-rule='evenodd' d='M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 12px center;
  background-size: 12px;
}

.reset-btn {
  background: #f1f5f9;
  color: #64748b;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}

.reset-btn:hover {
  background: #e2e8f0;
  color: #2563eb;
  transform: rotate(-45deg);
}

.filter-input:focus, .filter-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}

/* Sections */
.sections-container {
  display: flex;
  flex-direction: column;
  gap: 50px;
}

.job-section {
  background: #f8fafc;
  padding: 30px;
  border-radius: 20px;
  border: 1px solid var(--border);
}

.section-title {
  font-size: 20px;
  font-weight: 800;
  margin-bottom: 25px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.collapsible-header {
  cursor: pointer;
  justify-content: space-between;
  user-select: none;
  transition: opacity 0.2s;
}

.collapsible-header:hover {
  opacity: 0.8;
}

.title-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.collapse-arrow {
  font-size: 18px;
  color: #94a3b8;
  transition: transform 0.3s;
}

.section-title i {
  font-size: 24px;
}

.pending-title { color: #f59e0b; }
.approved-title { color: #10b981; }
.rejected-title { color: #ef4444; }

.count-badge {
  background: #e2e8f0;
  color: #475569;
  font-size: 14px;
  padding: 2px 10px;
  border-radius: 20px;
  margin-left: 5px;
}

.no-data-inline {
  padding: 30px;
  text-align: center;
  background: white;
  border-radius: 12px;
  color: var(--text-muted);
  font-style: italic;
  border: 1px dashed var(--border);
}

.status-badge.rejected {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

/* Grid Layout */
.jobs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
}

/* Job Card */
.job-card {
  background: var(--bg-card);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
  transition: all 0.3s ease;
  border: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  position: relative;
}

.job-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(0,0,0,0.1);
}

.pending-border {
  border-top: 5px solid #f59e0b;
}

.approved-border {
  border-top: 5px solid #10b981;
}

.rejected-border {
  border-top: 5px solid #ef4444;
}

.card-header-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.company-badge {
  font-size: 13px;
  font-weight: 600;
  color: #3b82f6;
  background: rgba(59, 130, 246, 0.1);
  padding: 4px 12px;
  border-radius: 20px;
}

.status-badge {
  font-size: 12px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 6px;
  text-transform: uppercase;
}

.status-badge.approved {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.status-badge.pending {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.job-title {
  font-size: 19px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 15px;
  line-height: 1.3;
}

.job-meta {
  margin-bottom: 20px;
  flex-grow: 1;
}

.meta-item {
  display: flex;
  align-items: center;
  font-size: 14px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.meta-item i {
  margin-right: 10px;
  font-size: 16px;
  color: #64748b;
}

.card-footer {
  display: flex;
  gap: 12px;
  padding-top: 15px;
  border-top: 1px solid var(--border);
}

.detail-btn {
  flex: 1;
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #e2e8f0;
  padding: 10px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.detail-btn:hover {
  background: #e2e8f0;
  transform: translateY(-1px);
}

.approve-card-btn {
  flex: 1;
  background: #dcfce7;
  color: #15803d;
  border: 1px solid #bbf7d0;
  padding: 10px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.approve-card-btn:hover {
  background: #10b981;
  color: white;
  border-color: #10b981;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.2);
}

.reject-card-btn {
  flex: 1;
  background: #fff1f2;
  color: #e11d48;
  border: 1px solid #fecdd3;
  padding: 10px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.reject-card-btn:hover {
  background: #e11d48;
  color: white;
  border-color: #e11d48;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(225, 29, 72, 0.2);
}

.restore-card-btn {
  flex: 1;
  background: #f0f9ff;
  color: #0369a1;
  border: 1px solid #bae6fd;
  padding: 10px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.restore-card-btn:hover {
  background: #0ea5e9;
  color: white;
  border-color: #0ea5e9;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.2);
}

.no-data-card {
  text-align: center;
  padding: 60px;
  background: var(--bg-card);
  border-radius: 16px;
  color: var(--text-muted);
}

.no-data-card i {
  font-size: 64px;
  margin-bottom: 15px;
  display: block;
}

/* Modal Styling */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

.modal-content {
  background: var(--bg-card);
  width: 100%;
  max-width: 650px;
  max-height: 90vh;
  border-radius: 20px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalScale 0.3s ease-out;
}

@keyframes modalScale {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.modal-header {
  padding: 20px 30px;
  border-bottom: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 {
  font-size: 1.5rem;
  margin: 0;
  color: var(--text-primary);
}

.close-modal-btn {
  background: #f1f5f9;
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
}

.close-modal-btn:hover {
  background: #fee2e2;
  color: #ef4444;
  transform: rotate(90deg);
}

.modal-body {
  padding: 30px;
  overflow-y: auto;
}

.detail-group {
  margin-bottom: 20px;
}

.detail-group label {
  display: block;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-muted);
  margin-bottom: 6px;
  letter-spacing: 0.05em;
}

.detail-value {
  font-size: 16px;
  color: var(--text-primary);
  font-weight: 500;
  margin: 0;
}

.detail-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.description-text {
  font-size: 15px;
  line-height: 1.6;
  color: var(--text-primary);
  white-space: pre-line;
  background: #f8fafc;
  padding: 15px;
  border-radius: 12px;
  border: 1px solid var(--border);
}

.skills-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.skill-tag {
  padding: 4px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  border: 1px solid transparent;
}
.tag-color-0 { background: rgba(59, 130, 246, 0.1); color: #2563eb; border-color: rgba(59, 130, 246, 0.2); }
.tag-color-1 { background: rgba(16, 185, 129, 0.1); color: #059669; border-color: rgba(16, 185, 129, 0.2); }
.tag-color-2 { background: rgba(245, 158, 11, 0.1); color: #d97706; border-color: rgba(245, 158, 11, 0.2); }
.tag-color-3 { background: rgba(139, 92, 246, 0.1); color: #7c3aed; border-color: rgba(139, 92, 246, 0.2); }
.tag-color-4 { background: rgba(236, 72, 153, 0.1); color: #db2777; border-color: rgba(236, 72, 153, 0.2); }

.modal-footer {
  padding: 20px 30px;
  border-top: 1px solid var(--border);
  display: flex;
  justify-content: flex-end;
  gap: 15px;
}

/* Keep current global styles but remove some now unused ones if needed */
.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  padding: 10px 24px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.2s;
}

.primary-btn:hover {
  background: #1d4ed8;
  transform: translateY(-1px);
}

.secondary-btn:hover {
  background: #e2e8f0;
}

.danger-btn {
  background: #ef4444;
  color: white;
  border: none;
  padding: 10px 24px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.3s;
}

.danger-btn:hover {
  background: #dc2626;
  transform: translateY(-1px);
}

</style>