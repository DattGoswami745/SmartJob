<template>
  <div class="company-reports-page">
    <!-- HEADER -->
    <div class="page-header-container">
      <div class="page-header d-flex justify-content-between align-items-center">
        <div class="header-left">
          <h2>Analytics & Reports</h2>
          <p class="text-muted">Generate detailed job application reports and insights.</p>
        </div>
        <div class="header-right">
          <button class="generate-btn" @click="showReportModal = true">
            <i class="bi bi-plus-lg"></i> Generate New Report
          </button>
        </div>
      </div>
    </div>

    <!-- RECENT REPORTS / INFO SECTION -->
    <div class="card-section info-card">
      <div class="d-flex align-items-center gap-4">
        <div class="info-icon">
          <i class="bi bi-file-earmark-spreadsheet-fill"></i>
        </div>
        <div>
          <h4>Export your data</h4>
          <p class="mb-0">You can export all application data or filter by a specific job posting to get a focused report in Excel format.</p>
        </div>
      </div>
    </div>

    <!-- REPORT MODAL -->
    <div v-if="showReportModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content report-modal">
        <div class="modal-header">
          <h3>Generate Report</h3>
          <button class="close-modal-btn" @click="closeModal">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        
        <div class="modal-body">
          <div class="selection-group">
            <label class="section-label">1. Choose Report Type</label>
            <div class="radio-options">
              <label class="radio-card" :class="{ active: reportType === 'all' }">
                <input type="radio" v-model="reportType" value="all" name="reportType" />
                <div class="radio-info">
                  <span class="radio-title">All Applications</span>
                  <span class="radio-desc">Export applications for every job posting you have.</span>
                </div>
                <div class="check-icon" v-if="reportType === 'all'"><i class="bi bi-check-circle-fill"></i></div>
              </label>

              <label class="radio-card" :class="{ active: reportType === 'specific' }">
                <input type="radio" v-model="reportType" value="specific" name="reportType" />
                <div class="radio-info">
                  <span class="radio-title">Specific Job</span>
                  <span class="radio-desc">Export applications for one specific job posting.</span>
                </div>
                <div class="check-icon" v-if="reportType === 'specific'"><i class="bi bi-check-circle-fill"></i></div>
              </label>
            </div>
          </div>

          <div class="selection-group mt-4 animate-fade-in" v-if="reportType === 'specific'">
            <label class="section-label">2. Select the Job</label>
            <div class="select-wrapper">
              <i class="bi bi-briefcase select-icon"></i>
              <select v-model="selectedJobId" class="custom-select">
                <option value="" disabled>--- Select a Job ---</option>
                <option v-for="job in jobs" :key="job.jobId" :value="job.jobId">
                  {{ job.title }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="cancel-btn" @click="closeModal">Cancel</button>
          <button 
            class="download-btn-primary" 
            :disabled="reportType === 'specific' && !selectedJobId || downloading"
            @click="handleDownload"
          >
            <span v-if="downloading" class="spinner-border spinner-border-sm me-2"></span>
            <i v-else class="bi bi-file-earmark-excel me-2"></i>
            Download Excel
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from "vue"
import { getCompanyJobs, downloadJobReport, downloadCompanyApplicationsReport } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"

export default {
  setup() {
    const showReportModal = ref(false)
    const reportType = ref("all")
    const selectedJobId = ref("")
    const jobs = ref([])
    const downloading = ref(false)

    const loadJobs = async () => {
      try {
        jobs.value = await getCompanyJobs()
      } catch (err) {
        handleError(err, "Load Error")
      }
    }

    const closeModal = () => {
      showReportModal.value = false
      selectedJobId.value = ""
      reportType.value = "all"
    }

    const handleDownload = async () => {
      try {
        downloading.value = true
        if (reportType.value === "all") {
          // Providing empty strings as default filters to get all applications
          await downloadCompanyApplicationsReport("", "")
        } else {
          await downloadJobReport(selectedJobId.value)
        }
        handleSuccess("Report generated successfully!")
        closeModal()
      } catch (err) {
        handleError(err, "Generation Failed")
      } finally {
        downloading.value = false
      }
    }

    onMounted(loadJobs)

    return {
      showReportModal,
      reportType,
      selectedJobId,
      jobs,
      downloading,
      closeModal,
      handleDownload
    }
  }
}
</script>

<style scoped>
.company-reports-page {
  padding-bottom: 50px;
}

/* Header */
.page-header-container {
  background: var(--bg-card);
  padding: 24px 30px;
  border-radius: 20px;
  border: 1px solid var(--border);
  margin-bottom: 30px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.page-header h2 {
  font-weight: 800;
  margin: 0;
  color: var(--text-primary);
  font-size: 28px;
}

.generate-btn {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 14px;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
}

.generate-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(245, 158, 11, 0.4);
  filter: brightness(1.1);
}

/* Info Card */
.info-card {
  padding: 40px;
  border-top: 5px solid #f59e0b;
  display: flex;
  background: var(--bg-card);
  border-radius: 20px;
  border-left: 1px solid var(--border);
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.info-icon {
  width: 70px;
  height: 70px;
  background: #fef3c7;
  color: #f59e0b;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
}

.info-card h4 {
  font-weight: 700;
  margin-bottom: 8px;
}

/* Modal */
.modal-overlay {
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
  z-index: 2000;
}

.report-modal {
  background: var(--bg-card);
  width: 90%;
  max-width: 500px;
  border-radius: 28px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
  overflow: hidden;
  animation: modalPop 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes modalPop {
  0% { transform: scale(0.9) translateY(20px); opacity: 0; }
  100% { transform: scale(1) translateY(0); opacity: 1; }
}

.modal-header {
  padding: 24px 30px;
  border-bottom: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fcfcfc;
}

.modal-header h3 {
  margin: 0;
  font-weight: 800;
  font-size: 20px;
}

.close-modal-btn {
  background: #f1f5f9;
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 12px;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
}

.modal-body {
  padding: 30px;
}

.section-label {
  display: block;
  font-weight: 700;
  font-size: 14px;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 16px;
}

.radio-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.radio-card {
  position: relative;
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border: 2px solid var(--border);
  border-radius: 18px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.radio-card input {
  display: none;
}

.radio-card:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.radio-card.active {
  border-color: #f59e0b;
  background: #fffcf5;
}

.radio-info {
  display: flex;
  flex-direction: column;
}

.radio-title {
  font-weight: 700;
  font-size: 16px;
  color: var(--text-primary);
}

.radio-desc {
  font-size: 13px;
  color: #64748b;
  margin-top: 2px;
}

.check-icon {
  margin-left: auto;
  color: #f59e0b;
  font-size: 20px;
}

/* Select Styling */
.select-wrapper {
  position: relative;
}

.select-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  pointer-events: none;
}

.custom-select {
  width: 100%;
  padding: 12px 16px 12px 45px;
  border-radius: 14px;
  border: 2px solid var(--border);
  background: white;
  font-size: 15px;
  font-weight: 500;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%2394a3b8' class='bi bi-chevron-down' viewBox='0 0 16 16'%3E%3Cpath fill-rule='evenodd' d='M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 16px center;
  background-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.custom-select:focus {
  border-color: #f59e0b;
  outline: none;
  box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.1);
}

.modal-footer {
  padding: 20px 30px;
  background: #f8fafc;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  border-top: 1px solid var(--border);
}

.cancel-btn {
  background: white;
  border: 1px solid var(--border);
  padding: 10px 20px;
  border-radius: 12px;
  font-weight: 600;
  color: #64748b;
}

.download-btn-primary {
  background: #1e293b;
  color: white;
  border: none;
  padding: 10px 24px;
  border-radius: 12px;
  font-weight: 700;
  display: flex;
  align-items: center;
  transition: all 0.2s;
}

.download-btn-primary:hover:not(:disabled) {
  background: #0f172a;
  transform: translateY(-1px);
}

.download-btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.animate-fade-in {
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
