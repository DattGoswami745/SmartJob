<template>
  <div class="user-dashboard-wrapper">
    <div class="dashboard-header mb-5">
      <h2 class="fw-bold m-0 gradient-text">My Applications</h2>
      <p class="text-muted mt-1">Track the status of roles you have applied for.</p>
    </div>

    <!-- Empty State -->
    <div v-if="applications.length === 0" class="premium-table-card text-center py-5">
      <div class="empty-state">
        <FileX size="48" class="text-muted mb-3 opacity-50 mx-auto d-block" />
        <h5 class="fw-bold text-main mb-2">No Applications Found</h5>
        <p class="text-muted m-0">You haven't applied to any jobs yet. Browse available jobs to get started!</p>
        <router-link to="/jobs" class="btn btn-primary-gradient mt-4 px-4 py-2">
          Browse Jobs
        </router-link>
      </div>
    </div>

    <div class="row g-4 mb-5" v-else>
      <div
        v-for="app in applications"
        :key="app.applicationId"
        class="col-md-6 col-lg-4"
      >
        <div class="premium-card">
          <div class="card-content flex-column align-items-start gap-3 h-100">
            <!-- Header -->
            <div class="d-flex justify-content-between align-items-start w-100 mb-2">
              <div class="d-flex align-items-center gap-3">
                <div class="job-avatar flex-shrink-0">
                  <Briefcase size="20" class="text-primary" />
                </div>
                <div>
                  <h6 class="fw-bold m-0 text-main line-clamp-1" :title="app.title">{{ app.title || 'Job Title' }}</h6>
                  <span class="text-muted small d-block line-clamp-1" :title="app.company">{{ app.company || 'Company Name' }}</span>
                </div>
              </div>
            </div>

            <div class="w-100 border-top border-light my-1"></div>

            <div class="d-flex justify-content-between align-items-center w-100 mt-auto pt-2">
              <span class="text-muted small fw-medium d-flex align-items-center gap-1">
                <Clock size="14" />
                Status
              </span>
              <span
                class="badge rounded-pill px-3 py-2 fw-semibold"
                :class="statusClass(app.status)"
              >
                {{ app.status || 'Pending' }}
              </span>
            </div>
          </div>
          <!-- Colored glow line based on status -->
          <div class="card-glow" :class="statusGlowClass(app.status)"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { getMyApplications } from "../services/api"
import { Briefcase, FileX, Clock } from "lucide-vue-next"

const applications = ref([])

onMounted(async () => {
  try {
    applications.value = await getMyApplications()
  } catch (err) {
    console.error("Failed to load applications", err)
  }
})

function statusClass(status) {
  if (status === "Applied" || status === "Pending") return "bg-warning-subtle text-warning-emphasis"
  if (status === "Shortlisted") return "bg-success-subtle text-success-emphasis"
  if (status === "Rejected") return "bg-danger-subtle text-danger-emphasis"
  return "bg-secondary-subtle text-secondary-emphasis"
}

function statusGlowClass(status) {
  if (status === "Applied" || status === "Pending") return "glow-warning"
  if (status === "Shortlisted") return "glow-success"
  if (status === "Rejected") return "glow-danger"
  return "glow-secondary"
}
</script>

<style scoped>
.user-dashboard-wrapper {
  padding: 10px;
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
  opacity: 0.8;
  transition: height 0.3s ease;
}

.premium-card:hover .card-glow {
  height: 100%;
  opacity: 0.05;
}

.glow-warning { background: linear-gradient(90deg, #f59e0b, #fbbf24); }
.glow-success { background: linear-gradient(90deg, #10b981, #34d399); }
.glow-danger { background: linear-gradient(90deg, #ef4444, #f87171); }
.glow-secondary { background: linear-gradient(90deg, #64748b, #94a3b8); }

/* Premium layout reuse */
.premium-table-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 30px;
  box-shadow: 0 15px 35px -15px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
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

/* Utilities */
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.text-main { color: var(--text-main); }
.border-light { border-color: var(--border) !important; }

/* Browse Jobs Button in empty state */
.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(59, 130, 246, 0.3);
  font-weight: 600;
  transition: all 0.3s ease;
  text-decoration: none;
  display: inline-block;
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
  color: white;
}

/* Colored Status Badges - Using custom overrides for light/dark mode consistency */
.bg-warning-subtle { background-color: rgba(245, 158, 11, 0.15) !important; }
.text-warning-emphasis { color: #d97706 !important; }

.bg-success-subtle { background-color: rgba(16, 185, 129, 0.15) !important; }
.text-success-emphasis { color: #059669 !important; }

.bg-danger-subtle { background-color: rgba(239, 68, 68, 0.15) !important; }
.text-danger-emphasis { color: #dc2626 !important; }

body.theme-dark .text-warning-emphasis { color: #fbbf24 !important; }
body.theme-dark .text-success-emphasis { color: #34d399 !important; }
body.theme-dark .text-danger-emphasis { color: #f87171 !important; }
</style>
