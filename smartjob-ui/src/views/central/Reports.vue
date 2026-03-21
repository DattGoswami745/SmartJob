<template>
  <div class="central-reports-page">
    <!-- CONSOLIDATED REPORT GENERATOR -->
    <div class="card-section mb-4 consolidated-report-section">
      <div class="section-title mb-3 d-flex align-items-center gap-2">
        <i class="bi bi-file-earmark-spreadsheet-fill text-success fs-4"></i>
        <h4 class="m-0 fw-bold">Consolidated Report Generator</h4>
      </div>
      
      <div class="row g-3 align-items-end">
        <div class="col-md-4">
          <label class="form-label small fw-bold text-muted">Select Company</label>
          <select v-model="selectedCompany" class="form-select custom-select">
            <option :value="null">All Companies</option>
            <option v-for="c in companies" :key="c.companyId" :value="c.companyId">
              {{ c.companyName }}
            </option>
          </select>
        </div>
        
        <div class="col-md-4">
          <label class="form-label small fw-bold text-muted">Select Job (Optional)</label>
          <select v-model="selectedJobId" class="form-select custom-select">
            <option :value="null">All Jobs</option>
            <option v-for="j in approvedJobs" :key="j.jobId" :value="j.jobId">
              {{ j.title }} ({{ j.companyName }})
            </option>
          </select>
        </div>
        
        <div class="col-md-4">
          <button 
            class="btn primary-btn w-100 py-2 d-flex align-items-center justify-content-center gap-2 consolidated-btn"
            @click="handleConsolidatedDownload"
            :disabled="reportLoading"
          >
            <span v-if="reportLoading" class="spinner-border spinner-border-sm"></span>
            <i v-else class="bi bi-cloud-arrow-down-fill"></i>
            {{ reportLoading ? 'Generating...' : 'Download Consolidated Report' }}
          </button>
        </div>
      </div>
    </div>

    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Job Applicant Reports</h2>
      </div>
      <div class="header-right">
        <input 
          type="text" 
          v-model="searchJob" 
          placeholder="Search by Job Title or Company..." 
          class="search-input"
        />
      </div>
    </div>

    <!-- JOBS TABLE -->
    <div class="card-section">
      <div class="table-wrapper">
        <table class="app-table">
          <thead>
            <tr>
              <th>Job Title</th>
              <th>Company</th>
              <th>Job Type</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="job in filteredJobs" :key="job.jobId">
              <td class="fw">{{ job.title }}</td>
              <td>{{ job.companyName || 'N/A' }}</td>
              <td>
                <span class="type-badge">{{ job.jobType }}</span>
              </td>
              <td>
                <span :class="['status-badge', job.isActive ? 'active' : 'inactive']">
                  {{ job.isActive ? 'Active' : 'Closed' }}
                </span>
              </td>
              <td>
                <button 
                  class="action-btn download-btn" 
                  @click="handleDownload(job.jobId, job.title)"
                >
                  <i class="bi bi-file-earmark-excel-fill"></i> Download Report
                </button>
              </td>
            </tr>
            <tr v-if="filteredJobs.length === 0">
              <td colspan="5" class="no-data text-center py-4">
                {{ loading ? 'Loading reports...' : 'No jobs matched your search.' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- NO APPLICANTS ERROR MODAL -->
    <div class="custom-modal-overlay" v-if="showErrorModal" @click.self="closeErrorModal">
      <div class="custom-modal">
        <div class="modal-header">
          <h3>No Applicants</h3>
          <button class="close-btn" @click="closeErrorModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body error-body">
          <i class="bi bi-exclamation-triangle-fill text-warning error-icon"></i>
          <p>{{ errorMessage }}</p>
        </div>
        <div class="modal-footer">
          <button class="primary-btn ok-btn" @click="closeErrorModal">OK</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue"
import { getCentralJobs, downloadJobReport, getCompanies, downloadCentralMultiReport } from "@/services/api"

const jobs = ref([])
const companies = ref([])
const searchJob = ref("")
const loading = ref(false)
const reportLoading = ref(false)

const selectedCompany = ref(null)
const selectedJobId = ref(null)

const showErrorModal = ref(false)
const errorMessage = ref("")

const loadData = async () => {
  try {
    loading.value = true
    const [jobsData, companiesData] = await Promise.all([
      getCentralJobs(),
      getCompanies()
    ])
    jobs.value = jobsData
    companies.value = companiesData
  } catch (err) {
    console.error("Error loading reports data:", err)
  } finally {
    loading.value = false
  }
}

const approvedJobs = computed(() => {
  let list = jobs.value.filter(j => j.isApproved)
  if (selectedCompany.value) {
    list = list.filter(j => j.companyId === selectedCompany.value)
  }
  return list
})

const filteredJobs = computed(() => {
  const list = jobs.value.filter(j => j.isApproved)
  if (!searchJob.value.trim()) return list
  
  const searchTerm = searchJob.value.toLowerCase()
  return list.filter(j => 
    j.title?.toLowerCase().includes(searchTerm) || 
    (j.companyName && j.companyName.toLowerCase().includes(searchTerm))
  )
})

const handleDownload = async (jobId, title) => {
  try {
    await downloadJobReport(jobId)
  } catch (err) {
    errorMessage.value = `Cannot generate report. ${err.message}`
    showErrorModal.value = true
  }
}

const handleConsolidatedDownload = async () => {
  try {
    reportLoading.value = true
    await downloadCentralMultiReport(selectedCompany.value, selectedJobId.value)
  } catch (err) {
    errorMessage.value = `Error: ${err.message}`
    showErrorModal.value = true
  } finally {
    reportLoading.value = false
  }
}

const closeErrorModal = () => {
  showErrorModal.value = false
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.central-reports-page {
  padding: 30px;
  background: var(--bg-main);
  min-height: 100vh;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  background: var(--bg-card);
  padding: 20px 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.03);
}

.header-left {
  display: flex;
  align-items: center;
}

.page-header h2 {
  margin: 0;
  font-weight: 700;
  color: var(--text-primary);
  font-size: 24px;
}

.search-input {
  padding: 10px 16px;
  border: 1px solid var(--border);
  background: var(--bg-card);
  color: var(--text-primary);
  border-radius: 8px;
  width: 250px;
  font-size: 14px;
  transition: all 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Cards */
.card-section {
  background: var(--bg-card);
  padding: 30px;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.05);
  margin-bottom: 25px;
  border-top: 4px solid #8b5cf6; /* Unique Purple color for Reports */
}

/* Consolidated Report Section */
.consolidated-report-section {
  border-top: 4px solid #10b981 !important; /* Green for consolidated action */
  transition: all 0.3s ease;
}

.consolidated-report-section:hover {
  box-shadow: 0 8px 30px rgba(0,0,0,0.08);
}

.section-title h4 {
  font-size: 18px;
  color: var(--text-primary);
}

.custom-select {
  border-radius: 8px;
  border: 1px solid var(--border);
  background-color: var(--bg-card);
  color: var(--text-primary);
  padding: 10px 12px;
  font-size: 14px;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.custom-select:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  outline: none;
}

.consolidated-btn {
  background: linear-gradient(135deg, #10b981, #059669) !important;
  color: white !important;
  border: none !important;
  font-weight: 600 !important;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(16, 185, 129, 0.2);
}

.consolidated-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(16, 185, 129, 0.3);
}

.consolidated-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

/* Table */
.table-wrapper {
  overflow-x: auto;
}

.app-table {
  width: 100%;
  border-collapse: collapse;
}

.app-table th {
  background: var(--recent-bg);
  text-align: left;
  padding: 14px 16px;
  font-weight: 600;
  font-size: 14px;
  color: var(--text-muted);
  border-bottom: 2px solid var(--recent-border);
}

.app-table td {
  padding: 14px 16px;
  border-bottom: 1px solid var(--border);
  font-size: 14px;
  color: var(--text-primary);
  vertical-align: middle;
}

.app-table tbody tr:hover {
  background: var(--recent-bg);
}

.fw {
  font-weight: 600;
  color: var(--text-primary);
}

.no-data {
  color: var(--text-muted);
  font-style: italic;
}

/* Badges */
.status-badge {
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.type-badge {
  background: var(--bg-main);
  color: var(--text-primary);
  border: 1px solid var(--border);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.active {
  background: #dcfce7;
  color: #16a34a;
}

.inactive {
  background: var(--recent-bg);
  color: var(--text-muted);
}

/* Buttons */
.action-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  border-radius: 6px;
  padding: 8px 12px;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.download-btn {
  color: #047857;
  background: #d1fae5;
}

.download-btn:hover {
  background: #a7f3d0;
  color: #065f46;
}

.download-btn i {
  font-size: 15px;
}

/* Modals */
.custom-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.custom-modal {
  background: var(--bg-card);
  width: 90%;
  max-width: 400px;
  border-radius: 16px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5);
  overflow: hidden;
  animation: modal-fade-in 0.3s ease-out forwards;
}

@keyframes modal-fade-in {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--recent-bg);
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.close-btn {
  background: none;
  border: none;
  font-size: 18px;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
}

.modal-body {
  padding: 24px;
  font-size: 15px;
  text-align: center;
}

.error-icon {
  font-size: 40px;
  margin-bottom: 15px;
  display: block;
}

.modal-footer {
  padding: 16px 20px;
  background: var(--bg-main);
  border-top: 1px solid var(--border);
  display: flex;
  justify-content: flex-end;
}

.primary-btn {
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  border: none;
}

.ok-btn {
  background: #3b82f6;
  color: white;
}

.fs-4 { font-size: 1.5rem !important; }
</style>
