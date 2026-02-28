<template>
  <div>
    <h2 class="mb-4">Central Dashboard</h2>

    <!-- ================= SUMMARY CARDS ================= -->
    <div class="row g-4">

      <div class="col-md-3">
        <div class="card shadow-sm hover-card border-primary">
          <div class="card-body">
            <h6 class="text-muted">Total Users</h6>
            <h4>{{ stats.totalUsers }}</h4>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm hover-card border-info">
          <div class="card-body">
            <h6 class="text-muted">Total Jobs</h6>
            <h4>{{ stats.totalJobs }}</h4>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm hover-card border-warning">
          <div class="card-body">
            <h6 class="text-muted">Total Applications</h6>
            <h4>{{ stats.totalApplications }}</h4>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm hover-card border-success">
          <div class="card-body">
            <h6 class="text-muted">Total Placements</h6>
            <h4 class="text-success">{{ stats.totalPlaced }}</h4>
          </div>
        </div>
      </div>

    </div>

    <!-- ================= LINE CHARTS ================= -->
    <div class="row mt-4 g-4">

      <!-- Row 1 (3 Charts) -->
      <div class="col-md-4">
        <div class="card p-3 chart-card">
          <h6>Users Growth</h6>
          <div class="chart-wrapper">
            <Line v-if="usersChart" :data="usersChart" :options="chartOptions" />
          </div>
        </div>
      </div>

      <div class="col-md-4">
        <div class="card p-3 chart-card">
          <h6>Jobs Growth</h6>
          <div class="chart-wrapper">
            <Line v-if="jobsChart" :data="jobsChart" :options="chartOptions" />
          </div>
        </div>
      </div>

      <div class="col-md-4">
        <div class="card p-3 chart-card">
          <h6>Applications Growth</h6>
          <div class="chart-wrapper">
            <Line v-if="applicationsChart" :data="applicationsChart" :options="chartOptions" />
          </div>
        </div>
      </div>

      <!-- Row 2 (Centered Chart) -->
      <div class="col-md-4 offset-md-4">
        <div class="card p-3 chart-card">
          <h6>Placements Growth</h6>
          <div class="chart-wrapper">
            <Line v-if="placedChart" :data="placedChart" :options="chartOptions" />
          </div>
        </div>
      </div>

    </div>

    <!-- ================= RECENT APPLICATIONS ================= -->
    <div class="card shadow-sm hover-card mt-4">
      <div class="card-body">
        <h5 class="mb-3">Recent Applications</h5>

        <table class="table table-hover align-middle">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Job</th>
              <th>Status</th>
              <th>Date</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="app in recentApplications" :key="app.applicationId">
              <td>{{ app.userName }}</td>
              <td>{{ app.email }}</td>
              <td>{{ app.jobTitle }}</td>
              <td>
                <span
                  class="badge"
                  :class="{
                    'bg-warning': app.status === 'Applied' || app.status === 'Pending',
                    'bg-success': app.status === 'Placed',
                    'bg-danger': app.status === 'Rejected'
                  }"
                >
                  {{ app.status }}
                </span>
              </td>
              <td>{{ app.appliedDate?.split('T')[0] }}</td>
            </tr>

            <tr v-if="recentApplications.length === 0">
              <td colspan="5" class="text-center text-muted">
                No applications found
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { Line } from "vue-chartjs"
import {
  Chart,
  LineElement,
  PointElement,
  LinearScale,
  Title,
  CategoryScale,
  Tooltip,
  Legend
} from "chart.js"
import { getCentralDashboardData } from "@/services/api"

Chart.register(
  LineElement,
  PointElement,
  LinearScale,
  Title,
  CategoryScale,
  Tooltip,
  Legend
)

const stats = ref({
  totalUsers: 0,
  totalJobs: 0,
  totalApplications: 0,
  totalPlaced: 0
})

const recentApplications = ref([])

const usersChart = ref(null)
const jobsChart = ref(null)
const applicationsChart = ref(null)
const placedChart = ref(null)

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    }
  }
}

onMounted(async () => {
  try {
    const data = await getCentralDashboardData()

    stats.value = data.stats
    recentApplications.value = data.recentApplications

    // Handle missing or empty dailyChart array gracefully
    const chartData = data.dailyChart || []
    const labels = chartData.map(x => x.day)

    // Applications Growth (REAL DATA - Daily)
    applicationsChart.value = {
      labels,
      datasets: [{
        data: chartData.map(x => x.totalApplications),
        borderColor: "#ffc107",
        tension: 0.4
      }]
    }

    // Placements Growth (REAL DATA - Daily)
    placedChart.value = {
      labels,
      datasets: [{
        data: chartData.map(x => x.totalPlaced),
        borderColor: "#198754",
        tension: 0.4
      }]
    }

    // Optional: For now using totals for users & jobs (you can extend backend later)
    usersChart.value = {
      labels,
      datasets: [{
        data: labels.map(() => stats.value.totalUsers),
        borderColor: "#0d6efd",
        tension: 0.4
      }]
    }

    jobsChart.value = {
      labels,
      datasets: [{
        data: labels.map(() => stats.value.totalJobs),
        borderColor: "#17a2b8",
        tension: 0.4
      }]
    }

  } catch (err) {
    console.error("Central Dashboard load failed", err)
  }
})
</script>

<style scoped>
.hover-card {
  transition: all 0.3s ease;
  background-color: var(--bg-card);
  border: 1px solid var(--border);
}

.hover-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 1rem 2rem rgba(0,0,0,0.15);
}

.chart-card {
  height: 280px;
  background-color: var(--bg-card);
  border: 1px solid var(--border);
}

.chart-wrapper {
  height: 220px;
  position: relative;
}

.card {
  border-radius: 12px;
}

h2, h4, h5, h6 {
  color: var(--text-primary);
}

/* Ensure table adheres to theme */
.table {
  color: var(--text-primary);
}

.table th {
  color: var(--text-muted);
  border-bottom: 1px solid var(--border);
}

.table td {
  border-bottom: 1px solid var(--border);
}
</style>