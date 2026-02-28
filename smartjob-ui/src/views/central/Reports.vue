<template>
  <div class="central-reports-page">
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
              <td colspan="5" class="no-data">
                {{ jobs.length === 0 ? 'Loading jobs...' : 'No jobs match your search.' }}
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

<script>
import { ref, onMounted, computed } from "vue"
import { getCentralJobs, downloadJobReport } from "@/services/api"

export default {
  setup() {
    const jobs = ref([])
    const searchJob = ref("")
    const loading = ref(false)

    const showErrorModal = ref(false)
    const errorMessage = ref("")

    const loadJobs = async () => {
      try {
        loading.value = true
        jobs.value = await getCentralJobs()
      } catch (err) {
        alert("Error loading jobs: " + err.message)
      } finally {
        loading.value = false
      }
    }

    const filteredJobs = computed(() => {
      if (!searchJob.value.trim()) return jobs.value
      
      const searchTerm = searchJob.value.toLowerCase()
      return jobs.value.filter(j => 
        j.title?.toLowerCase().includes(searchTerm) || 
        (j.companyName && j.companyName.toLowerCase().includes(searchTerm))
      )
    })

    const handleDownload = async (jobId, title) => {
      console.log(`Triggering download for: ${title} (Job ID: ${jobId})`)
      try {
        await downloadJobReport(jobId)
      } catch (err) {
        errorMessage.value = `Cannot generate report. ${err.message}`
        showErrorModal.value = true
      }
    }

    const closeErrorModal = () => {
      showErrorModal.value = false
    }

    onMounted(() => {
      loadJobs()
    })

    return {
      jobs,
      filteredJobs,
      searchJob,
      loading,
      handleDownload,
      showErrorModal,
      errorMessage,
      closeErrorModal
    }
  }
}
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
  text-align: center;
  padding: 30px;
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

/* ================================= */
/* MODALS */
/* ================================= */

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
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.3);
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
  color: var(--text-primary);
}

.close-btn {
  background: none;
  border: none;
  font-size: 18px;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 50%;
  transition: all 0.2s;
  display: flex;
}

.close-btn:hover {
  background: var(--border);
  color: var(--text-primary);
}

.modal-body {
  padding: 24px;
  font-size: 15px;
  color: var(--text-primary);
  line-height: 1.5;
}

.error-body {
  text-align: center;
  padding-bottom: 10px;
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
  transition: all 0.2s;
  border: none;
}

.ok-btn {
  background: #3b82f6;
  color: white;
}

.ok-btn:hover {
  background: #2563eb;
}
</style>
