<template>
  <div class="central-applications-page">
    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Central Applications</h2>
      </div>
      <div class="header-right">
        <input 
          type="text" 
          v-model="searchCompany" 
          placeholder="Filter by Company Name..." 
          class="search-input"
        />
      </div>
    </div>

    <!-- APPLICATIONS TABLE -->
    <div class="card-section">
      <div class="table-wrapper">
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
                <button class="action-btn view-btn" @click="openProfileModal(app)">
                  <i class="bi bi-person-lines-fill"></i> View Profile
                </button>
                <button class="action-btn delete-btn" @click="openDeleteModal(app.applicationId)">
                  <i class="bi bi-trash-fill"></i> Remove
                </button>
              </td>
            </tr>
            <tr v-if="filteredApplications.length === 0">
              <td colspan="6" class="no-data">
                {{ applications.length === 0 ? 'No applications found.' : 'No applications match your search.' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- REMOVE CONFIRMATION MODAL -->
    <div class="custom-modal-overlay" v-if="showDeleteModal" @click.self="closeDeleteModal">
      <div class="custom-modal">
        <div class="modal-header">
          <h3>Remove Application</h3>
          <button class="close-btn" @click="closeDeleteModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body">
          <p>Are you sure you want to remove this application? This action cannot be undone.</p>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="closeDeleteModal">Cancel</button>
          <button class="primary-btn delete-confirm-btn" @click="confirmDelete" :disabled="loading">
            <span class="spinner-border spinner-border-sm me-2" v-if="loading"></span>
            {{ loading ? "Removing..." : "Yes, Remove" }}
          </button>
        </div>
      </div>
    </div>

    <!-- VIEW PROFILE MODAL -->
    <div class="custom-modal-overlay" v-if="showProfileModal" @click.self="closeProfileModal">
      <div class="custom-modal profile-modal">
        <div class="modal-header">
          <h3>Applicant Profile</h3>
          <button class="close-btn" @click="closeProfileModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body">
          <div v-if="loadingProfile" class="loading-state">
            <span class="spinner-border text-primary"></span>
            <p>Loading profile details...</p>
          </div>
          
          <div v-else-if="profileData">
            <div class="profile-header-info">
              <div class="basic-info">
                <h4>{{ selectedApp.fullName }}</h4>
                <p><i class="bi bi-envelope-fill text-muted"></i> {{ selectedApp.email }}</p>
                <p><i class="bi bi-geo-alt-fill text-muted"></i> {{ profileData.preferredLocation || 'Location not specified' }}</p>
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
              <a :href="profileData.resumePath" target="_blank" class="download-resume-btn">
                <i class="bi bi-file-earmark-pdf-fill"></i> View / Download Resume
              </a>
            </div>
          </div>
          
          <div v-else class="no-data-state">
            <i class="bi bi-exclamation-circle text-warning fs-1 mb-2"></i>
            <p>This user has not completed their profile yet.</p>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import { ref, onMounted, computed } from "vue"
import { getAllApplications, deleteCentralApplication, getUserProfileForAdmin } from "@/services/api"

export default {
  setup() {
    const applications = ref([])
    const searchCompany = ref("")
    const loading = ref(false)

    // Delete Modal State
    const showDeleteModal = ref(false)
    const appToDelete = ref(null)

    // Profile Modal State
    const showProfileModal = ref(false)
    const loadingProfile = ref(false)
    const selectedApp = ref(null)
    const profileData = ref(null)

    const loadApplications = async () => {
      try {
        loading.value = true
        applications.value = await getAllApplications()
      } catch (err) {
        alert("Error loading applications: " + err.message)
      } finally {
        loading.value = false
      }
    }

    // --- DELETE LOGIC ---
    const openDeleteModal = (appId) => {
      appToDelete.value = appId
      showDeleteModal.value = true
    }

    const closeDeleteModal = () => {
      showDeleteModal.value = false
      appToDelete.value = null
    }

    const confirmDelete = async () => {
      if (!appToDelete.value) return

      try {
        loading.value = true
        await deleteCentralApplication(appToDelete.value)
        applications.value = applications.value.filter(a => a.applicationId !== appToDelete.value)
        closeDeleteModal()
      } catch (err) {
        alert("Error deleting application: " + err.message)
      } finally {
        loading.value = false
      }
    }

    // --- VIEW PROFILE LOGIC ---
    const openProfileModal = async (app) => {
      selectedApp.value = app
      showProfileModal.value = true
      profileData.value = null

      try {
        loadingProfile.value = true
        // Fetch real profile data using the specific applicant's userId
        profileData.value = await getUserProfileForAdmin(app.userId)
      } catch (err) {
        console.error("User profile empty or error fetching", err)
      } finally {
        loadingProfile.value = false
      }
    }

    const closeProfileModal = () => {
      showProfileModal.value = false
      selectedApp.value = null
      profileData.value = null
    }

    const filteredApplications = computed(() => {
      if (!searchCompany.value.trim()) return applications.value
      
      const searchTerm = searchCompany.value.toLowerCase()
      return applications.value.filter(app => 
        app.companyName?.toLowerCase().includes(searchTerm)
      )
    })

    const formatDate = (dateString) => {
      if (!dateString) return ""
      const date = new Date(dateString)
      return date.toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    }

    onMounted(() => {
      loadApplications()
    })

    return {
      applications,
      filteredApplications,
      searchCompany,
      loading,
      
      // Delete Modal
      showDeleteModal,
      openDeleteModal,
      closeDeleteModal,
      confirmDelete,

      // Profile Modal
      showProfileModal,
      loadingProfile,
      selectedApp,
      profileData,
      openProfileModal,
      closeProfileModal,

      formatDate
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
  border-top: 4px solid #8b5cf6; /* Distinct purple to separate from Jobs */
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

.company-badge {
  background: var(--recent-bg);
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--text-muted);
  border: 1px solid var(--border);
}

.no-data {
  text-align: center;
  padding: 30px;
  color: var(--text-muted);
  font-style: italic;
}

/* Buttons */
.action-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  border-radius: 6px;
  padding: 6px 12px;
  transition: all 0.2s;
  margin-right: 8px;
}

.view-btn {
  color: #0284c7;
  background: #e0f2fe;
}

.view-btn:hover {
  background: #bae6fd;
}

.delete-btn {
  color: #e11d48;
  background: #ffe4e6;
}

.delete-btn:hover {
  background: #fecdd3;
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
  max-width: 450px;
  border-radius: 16px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.3);
  overflow: hidden;
  animation: modal-fade-in 0.3s ease-out forwards;
}

.profile-modal {
  max-width: 550px;
}

@keyframes modal-fade-in {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  padding: 20px 24px;
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

.modal-footer {
  padding: 16px 24px;
  background: var(--bg-main);
  border-top: 1px solid var(--border);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* Base Modal Buttons */
.primary-btn, .secondary-btn {
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.secondary-btn {
  background: var(--recent-bg);
  color: var(--text-primary);
  border: 1px solid var(--border);
}

.secondary-btn:hover {
  background: var(--border);
}

.delete-confirm-btn {
  background: #e11d48;
  color: white;
}

.delete-confirm-btn:hover {
  background: #be123c;
}

/* ================================= */
/* PROFILE MODAL UI */
/* ================================= */

.loading-state, .no-data-state {
  text-align: center;
  padding: 40px 0;
  color: var(--text-muted);
}

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
}

.profile-details-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.detail-card {
  background: var(--bg-main);
  padding: 16px;
  border-radius: 12px;
  border: 1px solid var(--border);
}

.detail-card h5 {
  margin: 0 0 8px 0;
  font-size: 13px;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.detail-card p {
  margin: 0;
  font-weight: 600;
  color: var(--text-primary);
}

.highlight-text {
  color: #0ea5e9 !important;
  font-size: 18px;
}

.badges-container {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.skill-badge {
  background: var(--bg-main);
  color: var(--text-primary);
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  border: 1px solid var(--border);
}

h5 {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 12px;
  color: var(--text-primary);
}

.download-resume-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: #fef2f2;
  color: #ef4444;
  padding: 10px 16px;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 500;
  transition: all 0.2s;
}

.download-resume-btn:hover {
  background: #fee2e2;
  color: #dc2626;
}
</style>
