<template>
  <div class="company-dashboard">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h2 class="fw-bold gradient-text m-0">Company Dashboard</h2>
        <p class="text-muted small">Manage your jobs, placements, and see recent activity.</p>
      </div>
      <button class="btn btn-primary-gradient d-flex align-items-center gap-2 px-4 shadow-sm" @click="$router.push('/central/jobs')">
        <Plus size="18" /> Post New Job
      </button>
    </div>

    <!-- ================= SUMMARY STATS ================= -->
    <div class="row g-4 mb-4">
      <div class="col-md-4">
        <div class="stat-card border-primary p-4 shadow-sm h-100">
          <div class="d-flex justify-content-between">
            <div>
              <p class="text-muted small fw-semibold text-uppercase mb-1">Active Jobs</p>
              <h3 class="fw-bold mb-0">{{ stats.totalJobs }}</h3>
            </div>
            <div class="icon-box bg-primary-soft">
              <Briefcase class="text-primary" size="24" />
            </div>
          </div>
          <div class="mt-3 d-flex align-items-center gap-2">
            <span class="badge bg-success-soft text-success small">+{{ dailyGrowth.jobs }} today</span>
          </div>
        </div>
      </div>

      <div class="col-md-4">
        <div class="stat-card border-success p-4 shadow-sm h-100">
          <div class="d-flex justify-content-between">
            <div>
              <p class="text-muted small fw-semibold text-uppercase mb-1">Total Placed</p>
              <h3 class="fw-bold mb-0 text-success">{{ stats.totalPlaced }}</h3>
            </div>
            <div class="icon-box bg-success-soft">
              <UserCheck class="text-success" size="24" />
            </div>
          </div>
          <div class="mt-3 d-flex align-items-center gap-2">
            <span class="badge bg-success-soft text-success small">+{{ dailyGrowth.placed }} today</span>
          </div>
        </div>
      </div>

      <div class="col-md-4">
        <div class="stat-card border-warning p-4 shadow-sm h-100">
          <div class="d-flex justify-content-between">
            <div>
              <p class="text-muted small fw-semibold text-uppercase mb-1">Total Submissions</p>
              <h3 class="fw-bold mb-0 text-warning">{{ stats.totalApplications }}</h3>
            </div>
            <div class="icon-box bg-warning-soft">
              <FileText class="text-warning" size="24" />
            </div>
          </div>
          <div class="mt-3 d-flex align-items-center gap-2">
            <span class="badge bg-warning-soft text-warning small">+{{ dailyGrowth.apps }} today</span>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <!-- ================= RECENT APPLICATIONS ================= -->
      <div class="col-lg-8">
        <div class="dashboard-card p-4 shadow-sm h-100">
          <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="fw-bold m-0 section-title">Recent Top Applications</h5>
            <router-link to="/central/applications" class="btn btn-light btn-sm px-3">View All</router-link>
          </div>

          <div class="table-responsive">
            <table class="table table-hover align-middle custom-table">
              <thead>
                <tr>
                  <th>Candidate</th>
                  <th>Job Title</th>
                  <th>Status</th>
                  <th>Applied On</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="app in recentApplications" :key="app.applicationId" class="table-row">
                  <td>
                    <div class="d-flex align-items-center gap-3">
                      <div class="avatar-sm bg-light text-primary fw-bold">
                        {{ app.userName?.charAt(0) || 'U' }}
                      </div>
                      <div>
                        <div class="fw-semibold">{{ app.userName }}</div>
                        <div class="text-muted x-small">{{ app.email }}</div>
                      </div>
                    </div>
                  </td>
                  <td><span class="fw-medium">{{ app.jobTitle }}</span></td>
                  <td>
                    <span :class="['badge rounded-pill', getStatusClass(app.status)]">
                      {{ app.status }}
                    </span>
                  </td>
                  <td>{{ formatDate(app.appliedDate) }}</td>
                </tr>
                <tr v-if="recentApplications.length === 0">
                  <td colspan="4" class="text-center py-5 text-muted">No recent applications found.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- ================= CHARTS ================= -->
      <div class="col-lg-4">
        <div class="dashboard-card p-4 shadow-sm h-100">
          <h5 class="fw-bold mb-4 section-title">Activity Overview</h5>
          <div class="chart-container">
             <Line v-if="chartReady" :data="chartData" :options="chartOptions" />
             <div v-else class="h-100 d-flex align-items-center justify-content-center text-muted">
               Loading chart...
             </div>
          </div>
          
          <div class="mt-4 pt-4 border-top">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <span class="small text-muted d-flex align-items-center gap-2">
                <span class="dot bg-primary"></span> Submissions
              </span>
              <span class="fw-bold">{{ stats.totalApplications }}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-2">
              <span class="small text-muted d-flex align-items-center gap-2">
                <span class="dot bg-success"></span> Placements
              </span>
              <span class="fw-bold">{{ stats.totalPlaced }}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center">
              <span class="small text-muted d-flex align-items-center gap-2">
                <span class="dot bg-warning"></span> Active Jobs
              </span>
              <span class="fw-bold">{{ stats.totalJobs }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue"
import { Briefcase, UserCheck, FileText, Plus } from "lucide-vue-next"
import { getCompanyDashboardData } from "@/services/api"
import { Line } from "vue-chartjs"
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from "chart.js"

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

const stats = ref({
  totalJobs: 0,
  totalApplications: 0,
  totalPlaced: 0
})

const recentApplications = ref([])
const dailyChartRaw = ref([])
const chartReady = ref(false)

const dailyGrowth = ref({ jobs: 0, placed: 0, apps: 0 })

const chartData = computed(() => {
  if (dailyChartRaw.value.length === 0) return {}
  
  const labels = dailyChartRaw.value.map(d => d.day)
  return {
    labels,
    datasets: [
      {
        label: 'Submissions',
        borderColor: '#3b82f6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        data: dailyChartRaw.value.map(d => d.totalApplications),
        fill: true,
        tension: 0.4
      },
      {
        label: 'Placements',
        borderColor: '#10b981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        data: dailyChartRaw.value.map(d => d.totalPlaced),
        fill: true,
        tension: 0.4
      }
    ]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      mode: 'index',
      intersect: false,
      backgroundColor: '#1e293b',
      padding: 12,
      bodySpacing: 8,
      usePointStyle: true
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { color: '#64748b', font: { size: 10 } }
    },
    y: {
      beginAtZero: true,
      grid: { color: 'rgba(148, 163, 184, 0.1)' },
      ticks: { color: '#64748b', font: { size: 10 }, stepSize: 1 }
    }
  }
}

async function loadData() {
  try {
    const res = await getCompanyDashboardData()
    stats.value = res.stats
    recentApplications.value = res.recentApplications
    dailyChartRaw.value = res.dailyChart
    
    // Calculate today's growth roughly from charts
    if (res.dailyChart.length > 0) {
       const today = res.dailyChart[res.dailyChart.length - 1]
       dailyGrowth.value = {
         jobs: today.totalJobs || 0,
         placed: today.totalPlaced || 0,
         apps: today.totalApplications || 0
       }
    }
    
    chartReady.value = true
  } catch (err) {
    console.error("Failed to load company dashboard data:", err)
  }
}

function getStatusClass(status) {
  if (status === 'Placed') return 'bg-success text-white'
  if (status === 'Pending' || status === 'Applied') return 'bg-warning-soft text-warning'
  if (status === 'Rejected') return 'bg-danger-soft text-danger'
  return 'bg-secondary text-white'
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A'
  return new Date(dateStr).toLocaleDateString()
}

onMounted(loadData)
</script>

<style scoped>
.company-dashboard {
  padding: 1rem;
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  font-weight: 600;
  border-radius: 12px;
  transition: all 0.3s;
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.25);
}

.stat-card {
  background: white;
  border-radius: 20px;
  border-left: 5px solid;
  transition: all 0.3s;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.border-primary { border-color: #3b82f6 !important; }
.border-success { border-color: #10b981 !important; }
.border-warning { border-color: #f59e0b !important; }

.icon-box {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.bg-primary-soft { background: rgba(59, 130, 246, 0.1); }
.bg-success-soft { background: rgba(16, 185, 129, 0.1); }
.bg-warning-soft { background: rgba(245, 158, 11, 0.1); }
.bg-danger-soft { background: rgba(239, 68, 68, 0.1); }

.dashboard-card {
  background: white;
  border-radius: 20px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.section-title {
  color: #1e293b;
  position: relative;
  padding-left: 15px;
}

.section-title::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 16px;
  background: #3b82f6;
  border-radius: 2px;
}

.custom-table th {
  background: #f8fafc;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  border-top: none;
}

.table-row {
  transition: background 0.2s;
}

.avatar-sm {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.x-small { font-size: 0.7rem; }

.chart-container {
  height: 250px;
  width: 100%;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}
</style>
