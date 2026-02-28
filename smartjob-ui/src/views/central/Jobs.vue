<template>
  <div class="jobs-page">

    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <button class="primary-btn" @click="toggleAddJob">
          <i class="bi bi-plus-circle-fill me-2"></i>
          {{ showAddJob ? "Close" : "Add Job" }}
        </button>
        <h2>Central Jobs</h2>
      </div>
    </div>

    <!-- ADD JOB SECTION -->
    <div v-if="showAddJob" class="card-section">
      <h3 class="section-title">Add Job</h3>

      <div class="form-grid">

        <!-- Company -->
        <div class="form-group">
          <label>Company</label>
          <div class="company-row">
            <select v-model.number="newJob.companyId">
              <option :value="null">Select Company</option>
              <option
                v-for="c in companies"
                :key="c.companyId"
                :value="c.companyId"
              >
                {{ c.companyName }}
              </option>
            </select>

            <button class="secondary-btn" @click="toggleAddCompany">
              {{ showAddCompany ? "Cancel" : "Add Company" }}
            </button>
          </div>
        </div>

        <!-- Add Company -->
        <div v-if="showAddCompany" class="nested-form">
          <input v-model="newCompany.companyName" placeholder="Company Name" />
          <input v-model="newCompany.industry" placeholder="Industry" />
          <input v-model="newCompany.location" placeholder="Location" />
          <button class="primary-btn" @click="createCompany">
            Save Company
          </button>
        </div>

        <div class="form-group">
          <label>Title</label>
          <input v-model="newJob.title" placeholder="Backend"/>
        </div>

        <div class="form-group full-width">
          <label>Description</label>
          <textarea v-model="newJob.description" placeholder="Description"></textarea>
        </div>

        <div class="form-group">
          <label>Required Skills</label>
          <input v-model="newJob.requiredSkills" placeholder="JAVA, C#"/>
        </div>

        <!-- 🔥 Job Type Dropdown -->
        <div class="form-group">
          <label>Job Type</label>
          <select v-model="newJob.jobType">
            <option value="">Select Job Type</option>
            <option>Full-Time</option>
            <option>Hybrid</option>
            <option>Remote</option>
          </select>
        </div>

        <div class="form-group">
          <label>Salary Range</label>
          <input v-model="newJob.salaryRange" placeholder="3-9 LPA"/>
        </div>

      </div>

      <div class="form-footer">
        <button class="primary-btn" @click="createJob" :disabled="loading">
          {{ loading ? "Saving..." : "Save Job" }}
        </button>
      </div>
    </div>

    <!-- JOB TABLE -->
    <div class="card-section">

      <h3 class="section-title">Job Listings</h3>

      <div class="table-wrapper">
        <table class="job-table">

          <thead>
            <tr>
              <th>Title</th>
              <th>Company</th>
              <th>Type</th>
              <th>Salary</th>
              <th>Skills</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="job in jobs" :key="job.jobId">
              <td class="fw">{{ job.title }}</td>
              <td>
                {{ companies.find(c => c.companyId === job.companyId)?.companyName || job.companyId }}
              </td>
              <td>{{ job.jobType }}</td>
              <td>{{ job.salaryRange }}</td>
              <td>{{ job.requiredSkills }}</td>
            </tr>

            <tr v-if="jobs.length === 0">
              <td colspan="5" class="no-data">
                No Jobs Available
              </td>
            </tr>

          </tbody>

        </table>
      </div>

    </div>

  </div>
</template>

<script>
import { ref, onMounted } from "vue"
import {
  getCentralJobs,
  addJob,
  getCentralCompanies,
  addCompany
} from "@/services/api"

export default {
  setup() {

    const jobs = ref([])
    const companies = ref([])
    const loading = ref(false)

    const showAddJob = ref(false)
    const showAddCompany = ref(false)

    const newJob = ref({
      companyId: null,
      title: "",
      description: "",
      requiredSkills: "",
      jobType: "",
      salaryRange: ""
    })

    const newCompany = ref({
      companyName: "",
      industry: "",
      location: ""
    })

    const loadJobs = async () => {
      try {
        jobs.value = await getCentralJobs()
      } catch (err) {
        alert("Error loading jobs: " + err.message)
      }
    }

    const loadCompanies = async () => {
      try {
        companies.value = await getCentralCompanies()
      } catch (err) {
        alert("Error loading companies: " + err.message)
      }
    }

    const toggleAddJob = () => {
      showAddJob.value = !showAddJob.value
    }

    const toggleAddCompany = () => {
      showAddCompany.value = !showAddCompany.value
    }

    const createCompany = async () => {
      try {
        if (!newCompany.value.companyName) {
          alert("Company name required")
          return
        }

        loading.value = true
        await addCompany(newCompany.value)

        alert("Company Added Successfully")

        newCompany.value = {
          companyName: "",
          industry: "",
          location: ""
        }

        showAddCompany.value = false
        await loadCompanies()
      } catch (err) {
        alert("Error: " + err.message)
      } finally {
        loading.value = false
      }
    }

    const createJob = async () => {
      try {
        if (!newJob.value.companyId) {
          alert("Select company")
          return
        }

        if (!newJob.value.title) {
          alert("Job title required")
          return
        }

        loading.value = true

        await addJob(newJob.value)

        alert("Job Added Successfully")

        newJob.value = {
          companyId: null,
          title: "",
          description: "",
          requiredSkills: "",
          jobType: "",
          salaryRange: ""
        }

        showAddJob.value = false
        await loadJobs()
      } catch (err) {
        alert("Error: " + err.message)
      } finally {
        loading.value = false
      }
    }

    onMounted(() => {
      loadJobs()
      loadCompanies()
    })

    return {
      jobs,
      companies,
      loading,
      showAddJob,
      showAddCompany,
      newJob,
      newCompany,
      toggleAddJob,
      toggleAddCompany,
      createJob,
      createCompany
    }
  }
}
</script>

<style scoped>

.jobs-page {
  padding: 30px;
  background: #f8fafc;
  min-height: 100vh;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.page-header h2 {
  margin: 0;
  font-weight: 600;
  color: #1e293b;
}

/* Buttons */
.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  padding: 8px 18px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
}

.primary-btn:hover {
  background: #1d4ed8;
}

.secondary-btn {
  background: #31ce23;
  color: white;
  border: none;
  padding: 8px 14px;
  border-radius: 8px;
  margin-left: 10px;
  cursor: pointer;
}

/* Cards */
.card-section {
  background: white;
  padding: 25px;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.05);
  margin-bottom: 25px;
}

.section-title {
  margin-bottom: 20px;
  font-weight: 600;
  color: #334155;
}

/* Form */
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 18px;
}

.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-size: 14px;
  font-weight: 500;
  color: #475569;
}

input, textarea, select {
  width: 100%;
  padding: 8px;
  margin-top: 6px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

textarea {
  min-height: 80px;
}

.form-footer {
  margin-top: 20px;
  text-align: right;
}

/* Nested Company */
.nested-form {
  background: #eef2ff;
  padding: 15px;
  border-radius: 10px;
  grid-column: 1 / -1;
}

/* Table */
.table-wrapper {
  overflow-x: auto;
}

.job-table {
  width: 100%;
  border-collapse: collapse;
}

.job-table th {
  background: #f1f5f9;
  text-align: left;
  padding: 12px;
  font-weight: 600;
  font-size: 14px;
  color: #475569;
}

.job-table td {
  padding: 12px;
  border-top: 1px solid #e2e8f0;
  font-size: 14px;
}

.job-table tbody tr:hover {
  background: #f8fafc;
}

.fw {
  font-weight: 600;
}

.no-data {
  text-align: center;
  padding: 20px;
  color: #94a3b8;
}

</style>