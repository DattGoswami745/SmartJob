<template>
  <div class="central-users-page">
    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Manage Users</h2>
      </div>
      <div class="header-right">
        <input 
          type="text" 
          v-model="searchUser" 
          placeholder="Filter by Name or Email..." 
          class="search-input"
        />
      </div>
    </div>

    <!-- USERS TABLE -->
    <div class="card-section">
      <div class="table-wrapper">
        <table class="app-table">
          <thead>
            <tr>
              <th>Full Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Joined Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.userId">
              <td class="fw">{{ user.fullName }}</td>
              <td>{{ user.email }}</td>
              <td>
                <span :class="['role-badge', user.role.toLowerCase() + '-role']">
                  {{ user.role }}
                </span>
              </td>
              <td>{{ formatDate(user.createdAt) }}</td>
              <td>
                <span :class="['status-badge', user.isActive ? 'active' : 'inactive']">
                  {{ user.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>
                <!-- Only allow viewing profile/activity if they are a regular 'User' -->
                <button 
                  v-if="user.role === 'User'" 
                  class="action-btn view-btn" 
                  @click="openProfileModal(user)"
                >
                  <i class="bi bi-person-lines-fill"></i> Profile
                </button>
                <button 
                  v-if="user.role === 'User'" 
                  class="action-btn activity-btn" 
                  @click="openActivityModal(user)"
                >
                  <i class="bi bi-activity"></i> Activity
                </button>
                <button 
                  class="action-btn delete-btn" 
                  @click="openDeleteModal(user.userId)"
                  :disabled="user.role === 'Central'"
                >
                  <i class="bi bi-trash-fill"></i> Remove
                </button>
              </td>
            </tr>
            <tr v-if="filteredUsers.length === 0">
              <td colspan="6" class="no-data">
                {{ users.length === 0 ? 'No users found.' : 'No users match your search.' }}
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
          <h3>Remove User</h3>
          <button class="close-btn" @click="closeDeleteModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body">
          <p>Are you sure you want to permanently delete this user? Their account and applications will be removed.</p>
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
          <h3>User Profile</h3>
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
                <h4>{{ selectedUser.fullName }}</h4>
                <p><i class="bi bi-envelope-fill text-muted"></i> {{ selectedUser.email }}</p>
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
              <a :href="`https://localhost:7269${profileData.resumePath}`" target="_blank" class="download-resume-btn">
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

    <!-- VIEW ACTIVITY MODAL -->
    <div class="custom-modal-overlay" v-if="showActivityModal" @click.self="closeActivityModal">
      <div class="custom-modal activity-modal">
        <div class="modal-header">
          <h3>User Activity Logs</h3>
          <button class="close-btn" @click="closeActivityModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body">
          <div v-if="loadingActivity" class="loading-state">
            <span class="spinner-border text-primary"></span>
            <p>Loading activity logs...</p>
          </div>
          
          <div v-else-if="activityData && activityData.length > 0" class="activity-timeline">
            <ul>
              <li v-for="log in activityData" :key="log.logId">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <p class="action-text">{{ log.action }}</p>
                  <span class="action-time">{{ formatDetailedDate(log.actionDate) }}</span>
                </div>
              </li>
            </ul>
          </div>
          
          <div v-else class="no-data-state">
            <i class="bi bi-clock-history text-muted fs-1 mb-2"></i>
            <p>No activity recorded for this user.</p>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import { ref, onMounted, computed } from "vue"
import { getCentralUsers, deleteCentralUser, getUserProfileForAdmin, getUserActivityLogs } from "@/services/api"

export default {
  setup() {
    const users = ref([])
    const searchUser = ref("")
    const loading = ref(false)

    // Modals State
    const showDeleteModal = ref(false)
    const userToDelete = ref(null)

    const showProfileModal = ref(false)
    const loadingProfile = ref(false)
    const selectedUser = ref(null)
    const profileData = ref(null)

    const showActivityModal = ref(false)
    const loadingActivity = ref(false)
    const activityData = ref([])

    const loadUsers = async () => {
      try {
        loading.value = true
        users.value = await getCentralUsers()
      } catch (err) {
        alert("Error loading users: " + err.message)
      } finally {
        loading.value = false
      }
    }

    // --- DELETE LOGIC ---
    const openDeleteModal = (userId) => {
      userToDelete.value = userId
      showDeleteModal.value = true
    }

    const closeDeleteModal = () => {
      showDeleteModal.value = false
      userToDelete.value = null
    }

    const confirmDelete = async () => {
      if (!userToDelete.value) return

      try {
        loading.value = true
        await deleteCentralUser(userToDelete.value)
        users.value = users.value.filter(u => u.userId !== userToDelete.value)
        closeDeleteModal()
      } catch (err) {
        alert("Error deleting user: " + err.message)
      } finally {
        loading.value = false
      }
    }

    // --- VIEW PROFILE LOGIC ---
    const openProfileModal = async (user) => {
      selectedUser.value = user
      showProfileModal.value = true
      profileData.value = null

      try {
        loadingProfile.value = true
        profileData.value = await getUserProfileForAdmin(user.userId)
      } catch (err) {
        console.error("User profile empty or error fetching", err)
      } finally {
        loadingProfile.value = false
      }
    }

    const closeProfileModal = () => {
      showProfileModal.value = false
      selectedUser.value = null
      profileData.value = null
    }

    // --- VIEW ACTIVITY LOGIC ---
    const openActivityModal = async (user) => {
      selectedUser.value = user
      showActivityModal.value = true
      activityData.value = []

      try {
        loadingActivity.value = true
        activityData.value = await getUserActivityLogs(user.userId)
      } catch (err) {
        console.error("Error fetching activity", err)
      } finally {
        loadingActivity.value = false
      }
    }

    const closeActivityModal = () => {
      showActivityModal.value = false
      selectedUser.value = null
      activityData.value = []
    }

    const filteredUsers = computed(() => {
      if (!searchUser.value.trim()) return users.value
      
      const searchTerm = searchUser.value.toLowerCase()
      return users.value.filter(u => 
        u.fullName?.toLowerCase().includes(searchTerm) || 
        u.email?.toLowerCase().includes(searchTerm)
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

    const formatDetailedDate = (dateString) => {
      if (!dateString) return ""
      const date = new Date(dateString)
      return date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    onMounted(() => {
      loadUsers()
    })

    return {
      users,
      filteredUsers,
      searchUser,
      loading,
      
      showDeleteModal,
      openDeleteModal,
      closeDeleteModal,
      confirmDelete,

      showProfileModal,
      loadingProfile,
      selectedUser,
      profileData,
      openProfileModal,
      closeProfileModal,

      showActivityModal,
      loadingActivity,
      activityData,
      openActivityModal,
      closeActivityModal,

      formatDate,
      formatDetailedDate
    }
  }
}
</script>

<style scoped>
.central-users-page {
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
  border-top: 4px solid #10b981; /* Distinct emerald color for Users */
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
.role-badge, .status-badge {
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.user-role {
  background: #e0e7ff;
  color: #4f46e5;
}

.central-role {
  background: #fcedea;
  color: #ea580c;
}

.active {
  background: #dcfce7;
  color: #16a34a;
}

.inactive {
  background: #f1f5f9;
  color: #64748b;
}

/* Buttons */
.action-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  border-radius: 6px;
  padding: 6px 10px;
  transition: all 0.2s;
  margin-right: 6px;
}

.view-btn {
  color: #0284c7;
  background: #e0f2fe;
}

.view-btn:hover {
  background: #bae6fd;
}

.activity-btn {
  color: #059669;
  background: #d1fae5;
}

.activity-btn:hover {
  background: #a7f3d0;
}

.delete-btn {
  color: #e11d48;
  background: #ffe4e6;
}

.delete-btn:hover {
  background: #fecdd3;
}

.delete-btn:disabled {
  background: #f1f5f9;
  color: #cbd5e1;
  cursor: not-allowed;
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

.profile-modal, .activity-modal {
  max-width: 550px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}

/* Required so activity timeline can scroll if long */
.activity-modal .modal-body {
  overflow-y: auto;
  max-height: 60vh;
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

/* ================================= */
/* ACTIVITY TIMELINE UI */
/* ================================= */

.activity-timeline ul {
  list-style-type: none;
  padding: 0;
  margin: 0;
  position: relative;
}

.activity-timeline ul::before {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  left: 10px;
  width: 2px;
  background: var(--border);
}

.activity-timeline li {
  position: relative;
  margin-bottom: 20px;
  padding-left: 36px;
}

.activity-timeline li:last-child {
  margin-bottom: 0;
}

.timeline-dot {
  position: absolute;
  left: 5px;
  top: 6px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #10b981;
  border: 2px solid var(--bg-card);
  z-index: 2;
}

.timeline-content {
  background: var(--bg-main);
  padding: 12px 16px;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
}

.action-text {
  margin: 0 0 6px 0;
  font-weight: 500;
  color: var(--text-primary);
  font-size: 14px;
}

.action-time {
  font-size: 12px;
  color: var(--text-muted);
}

</style>
