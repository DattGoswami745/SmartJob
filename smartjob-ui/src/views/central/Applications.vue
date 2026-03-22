<template>
  <div class="central-applications-page">
    <!-- HEADER & FILTERS -->
    <div class="page-header-container">
      <div class="page-header">
        <div class="header-left">
          <h2>Central Applications</h2>
        </div>
      </div>

      <div class="filters-bar">
        <!-- Search by Applicant -->
        <div class="search-box">
          <i class="bi bi-person-search"></i>
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="Search by applicant name..." 
            class="filter-input"
          />
        </div>

        <!-- Filter by Company -->
        <div class="filter-group">
          <i class="bi bi-building"></i>
          <input 
            type="text" 
            v-model="searchCompany" 
            placeholder="Search by company..." 
            class="filter-input"
          />
        </div>

        <!-- Filter by Job Title -->
        <div class="filter-group">
          <i class="bi bi-briefcase"></i>
          <input 
            type="text" 
            v-model="searchJob" 
            placeholder="Search by job title..." 
            class="filter-input"
          />
        </div>

        <button class="reset-btn" @click="resetFilters" title="Reset Filters">
          <i class="bi bi-arrow-counterclockwise"></i>
        </button>
      </div>
    </div>

    <!-- APPLICATIONS TABLE -->
    <div class="card-section">
      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status"></div>
        <p class="mt-2 text-muted">Loading applications...</p>
      </div>

      <div v-else class="table-wrapper">
        <table class="app-table">
          <thead>
            <tr>
              <th>Applicant Name</th>
              <th>Email</th>
              <th>Job Title</th>
              <th>Company</th>
              <th>Applied Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="app in filteredApplications" :key="app.applicationId">
              <td class="fw">{{ app.fullName }}</td>
              <td>{{ app.email }}</td>
              <td>{{ app.jobTitle }}</td>
              <td>
                <span class="company-badge">{{ app.companyName }}</span>
              </td>
              <td>{{ formatDate(app.appliedDate) }}</td>
              <td>
                <div class="action-buttons">
                  <button class="action-btn view-btn" @click="openProfileModal(app)" title="View Profile">
                    <i class="bi bi-person-lines-fill"></i> View
                  </button>
                  <button class="action-btn delete-btn" @click="openDeleteModal(app)" title="Remove Application">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredApplications.length === 0">
              <td colspan="6" class="no-data">
                {{ applications.length === 0 ? 'No applications found.' : 'No applications match your filtering.' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- DELETE MODAL -->
    <div v-if="appToDelete" class="modal-overlay" @click.self="appToDelete = null">
      <div class="modal-content mini-modal">
        <div class="modal-header">
          <h3>Remove Application</h3>
          <button class="close-modal-btn" @click="appToDelete = null">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        <div class="modal-body text-center">
          <div class="warning-icon">
            <i class="bi bi-exclamation-triangle"></i>
          </div>
          <p>Are you sure you want to remove <strong>{{ appToDelete.fullName }}</strong>'s application for <strong>{{ appToDelete.jobTitle }}</strong> at <strong>{{ appToDelete.companyName }}</strong>?</p>
          <p class="text-muted small">This action cannot be undone.</p>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="appToDelete = null">Cancel</button>
          <button class="danger-btn" @click="confirmDelete" :disabled="submitting">
            <span v-if="submitting" class="spinner-border spinner-border-sm me-2"></span>
            Remove Application
          </button>
        </div>
      </div>
    </div>

    <!-- PROFILE MODAL -->
    <div v-if="showProfileModal" class="modal-overlay" @click.self="closeProfileModal">
      <div class="modal-content profile-modal">
        <div class="modal-header">
          <h2>Applicant Profile</h2>
          <button class="close-modal-btn" @click="closeProfileModal">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        <div class="modal-body">
          <div v-if="loadingProfile" class="text-center py-4">
            <div class="spinner-border text-primary"></div>
            <p>Loading details...</p>
          </div>
          
          <div v-else-if="profileData">
            <div class="profile-header-info">
              <div class="basic-info">
                <h4>{{ selectedApp.fullName }}</h4>
                <p><i class="bi bi-envelope"></i> {{ selectedApp.email }}</p>
                <p><i class="bi bi-geo-alt"></i> {{ profileData.preferredLocation || 'Location not specified' }}</p>
              </div>
            </div>

            <div class="profile-details-grid">
              <div class="detail-card">
                <h5>Experience</h5>
                <p class="highlight-text">{{ profileData.experienceYears || 0 }} Years</p>
              </div>
              <div class="detail-card">
                <h5>Education</h5>
                <p>{{ profileData.education || 'Not specified' }}</p>
              </div>
            </div>

            <div class="skills-section mt-4">
              <h5>Skills & Technologies</h5>
              <div class="badges-container" v-if="profileData.skills">
                <span class="skill-badge" v-for="skill in profileData.skills.split(',')" :key="skill">
                  {{ skill.trim() }}
                </span>
              </div>
              <p v-else class="text-muted">No skills listed.</p>
            </div>

            <div class="resume-section mt-4" v-if="profileData.resumePath">
              <h5>Resume</h5>
              <button @click="openResumeViewer" class="download-resume-btn border-0 w-auto px-4">
                <i class="bi bi-file-earmark-pdf"></i> View Resume
              </button>
            </div>
          </div>
          
          <div v-else class="text-center py-4">
            <i class="bi bi-person-x text-warning fs-1 mb-2"></i>
            <p>This user has not completed their profile yet.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 📄 RESUME MODAL -->
    <ResumeModal 
      :isOpen="isResumeModalOpen" 
      :resumeUrl="resumeViewerUrl" 
      @close="isResumeModalOpen = false" 
    />
  </div>
</template>

<script>
import { ref, onMounted, computed } from "vue"
import { getAllApplications, deleteCentralApplication, getUserProfileForAdmin, API_HOST } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"
import ResumeModal from "@/components/ResumeModal.vue"

export default {
  components: { ResumeModal },
  setup() {
    const applications = ref([])
    const loading = ref(true)
    const submitting = ref(false)

    // Filters
    const searchQuery = ref("")
    const searchCompany = ref("")
    const searchJob = ref("")

    // Modals
    const appToDelete = ref(null)
    const showProfileModal = ref(false)
    const loadingProfile = ref(false)
    const selectedApp = ref(null)
    const profileData = ref(null)

    // Resume Viewer
    const isResumeModalOpen = ref(false)
    const resumeViewerUrl = ref("")

    const openResumeViewer = () => {
      if (profileData.value?.resumePath) {
        resumeViewerUrl.value = `${API_HOST}${profileData.value.resumePath}`
        isResumeModalOpen.value = true
      }
    }

    const loadApplications = async () => {
      try {
        loading.value = true
        applications.value = await getAllApplications()
      } catch (err) {
        handleError(err, "Load Error")
      } finally {
        loading.value = false
      }
    }

    const filteredApplications = computed(() => {
      let filtered = applications.value

      const qName = searchQuery.value.toLowerCase().trim()
      if (qName) {
        filtered = filtered.filter(a => a.fullName.toLowerCase().includes(qName))
      }

      const qCompany = searchCompany.value.toLowerCase().trim()
      if (qCompany) {
        filtered = filtered.filter(a => a.companyName?.toLowerCase().includes(qCompany))
      }

      const qJob = searchJob.value.toLowerCase().trim()
      if (qJob) {
        filtered = filtered.filter(a => a.jobTitle?.toLowerCase().includes(qJob))
      }

      return filtered
    })

    const resetFilters = () => {
      searchQuery.value = ""
      searchCompany.value = ""
      searchJob.value = ""
    }

    const openDeleteModal = (app) => {
      appToDelete.value = app
    }

    const confirmDelete = async () => {
      if (!appToDelete.value) return
      try {
        submitting.value = true
        await deleteCentralApplication(appToDelete.value.applicationId)
        applications.value = applications.value.filter(a => a.applicationId !== appToDelete.value.applicationId)
        handleSuccess("Application removed successfully")
        appToDelete.value = null
      } catch (err) {
        handleError(err, "Delete Error")
      } finally {
        submitting.value = false
      }
    }

    const openProfileModal = async (app) => {
      selectedApp.value = app
      showProfileModal.value = true
      profileData.value = null
      try {
        loadingProfile.value = true
        profileData.value = await getUserProfileForAdmin(app.userId)
      } catch (err) {
        console.error("Profile not found or error", err)
      } finally {
        loadingProfile.value = false
      }
    }

    const closeProfileModal = () => {
      showProfileModal.value = false
      selectedApp.value = null
      profileData.value = null
    }

    const formatDate = (dateString) => {
      if (!dateString) return ""
      return new Date(dateString).toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    }

    onMounted(loadApplications)

    return {
      applications,
      loading,
      submitting,
      searchQuery,
      searchCompany,
      searchJob,
      filteredApplications,
      resetFilters,
      appToDelete,
      openDeleteModal,
      confirmDelete,
      showProfileModal,
      loadingProfile,
      selectedApp,
      profileData,
      openProfileModal,
      closeProfileModal,
      formatDate,
      API_HOST,
      isResumeModalOpen,
      resumeViewerUrl,
      openResumeViewer
    }
  }
}
</script>

<style scoped>
.central-applications-page {
  padding: 30px;
  background: var(--bg-main);
  min-height: 100vh;
}

/* Header & Filters */
.page-header-container {
  background: var(--bg-card);
  padding: 24px;
  border-radius: 20px;
  border: 1px solid var(--border);
  margin-bottom: 30px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.page-header h2 {
  font-weight: 700;
  margin: 0 0 24px 0;
  color: var(--text-primary);
}

.filters-bar {
  display: flex;
  gap: 16px;
  align-items: center;
  flex-wrap: wrap;
}

.search-box, .filter-group {
  position: relative;
  flex: 1;
  min-width: 200px;
}

.search-box i, .filter-group i {
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
  color: var(--text-primary);
  transition: 0.2s;
}

.filter-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.reset-btn {
  background: #f1f5f9;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
}

.reset-btn:hover {
  background: #e2e8f0;
  color: #3b82f6;
}

/* Table */
.card-section {
  background: var(--bg-card);
  border-radius: 20px;
  border: 1px solid var(--border);
  border-top: 5px solid #ce0bf5ff;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0,0,0,0.04);
}

.table-wrapper {
  overflow-x: auto;
}

.app-table {
  width: 100%;
  border-collapse: collapse;
}

.app-table th {
  background: #f8fafc;
  padding: 16px 24px;
  text-align: left;
  font-size: 13px;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 1px solid var(--border);
}

.app-table td {
  padding: 16px 24px;
  border-bottom: 1px solid var(--border);
  vertical-align: middle;
  font-size: 14px;
  color: var(--text-primary);
}

.app-table tbody tr:hover {
  background: #f1f5f933;
}

.fw {
  font-weight: 600;
  color: var(--text-primary);
}

.company-badge {
  background: #f1f5f9;
  color: #475569;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  border: 1px solid var(--border);
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.action-btn {
  border: none;
  border-radius: 8px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.view-btn {
  background: #e0f2fe;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.view-btn:hover {
  background: #0ea5e9;
  color: white;
  border-color: #0ea5e9;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.2);
}

.delete-btn {
  background: #fff1f2;
  color: #e11d48;
  border: 1px solid #fecdd3;
}

.delete-btn:hover {
  background: #e11d48;
  color: white;
  border-color: #e11d48;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(225, 29, 72, 0.2);
}

/* Modals */
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

.modal-content {
  background: var(--bg-card);
  width: 90%;
  max-width: 600px;
  border-radius: 24px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
  animation: modalSlide 0.3s ease-out;
}

.mini-modal {
  max-width: 400px;
}

@keyframes modalSlide {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.modal-header {
  padding: 24px;
  border-bottom: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2, .modal-header h3 {
  margin: 0;
  font-weight: 700;
  color: var(--text-primary);
}

.close-modal-btn {
  background: #f1f5f9;
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 10px;
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
  padding: 24px;
}

.modal-footer {
  padding: 20px 24px;
  border-top: 1px solid var(--border);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.secondary-btn {
  padding: 10px 20px;
  border-radius: 12px;
  border: 1px solid var(--border);
  background: var(--bg-card);
  color: #64748b;
  font-weight: 500;
  cursor: pointer;
}

.danger-btn {
  padding: 10px 20px;
  border-radius: 12px;
  border: none;
  background: #ef4444;
  color: white;
  font-weight: 600;
  cursor: pointer;
}

.warning-icon {
  width: 60px;
  height: 60px;
  background: #fef2f2;
  color: #ef4444;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 30px;
  margin: 0 auto 16px;
}

/* Profile UI */
.profile-header-info {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px dashed var(--border);
}

.basic-info h4 {
  margin: 0 0 6px 0;
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
}

.basic-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 8px;
}

.profile-details-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.detail-card {
  background: var(--bg-main);
  padding: 16px;
  border-radius: 16px;
  border: 1px solid var(--border);
}

.detail-card h5 {
  margin: 0 0 8px 0;
  font-size: 12px;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.detail-card p {
  margin: 0;
  font-weight: 700;
  color: var(--text-primary);
}

.highlight-text {
  color: #3b82f6 !important;
  font-size: 18px;
}

.skill-badge {
  background: var(--bg-main);
  color: #2563eb;
  padding: 6px 14px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 500;
  margin-right: 8px;
  margin-bottom: 8px;
  display: inline-block;
  border: 1px solid var(--border);
}

.download-resume-btn {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  background: #fef2f2;
  color: #ef4444;
  padding: 12px 20px;
  border-radius: 12px;
  text-decoration: none;
  font-weight: 600;
  transition: 0.2s;
}

.download-resume-btn:hover {
  background: #fee2e2;
  transform: translateY(-2px);
}

.no-data {
  text-align: center;
  padding: 60px;
  color: #94a3b8;
  font-style: italic;
}
</style>
