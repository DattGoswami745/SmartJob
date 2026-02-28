<template>
  <div class="jobs-page">

    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Central Jobs</h2>
      </div>
      <div class="header-right">
        <button class="primary-btn add-job-btn" @click="toggleAddJob" v-if="!showUpdateJob">
          <i class="bi bi-plus-circle-fill me-2"></i>
          {{ showAddJob ? "Cancel Add" : "Add New Job" }}
        </button>
        <button class="secondary-btn add-job-btn" @click="cancelUpdate" v-else>
          <i class="bi bi-x-circle-fill me-2"></i> Cancel Update
        </button>
      </div>
    </div>

    <!-- ADD JOB SECTION -->
    <div v-if="showAddJob" class="card-section">
      <h3 class="section-title">Add Job</h3>

      <div class="form-grid">

        <!-- Company -->
        <div class="form-group">
          <label>Company <span class="required">*</span></label>
          <div class="company-row">
            <select v-model.number="newJob.companyId" :class="{ 'is-invalid': errors.companyId }">
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
          <span class="error-msg" v-if="errors.companyId">{{ errors.companyId }}</span>
        </div>

        <!-- Add Company -->
        <div v-if="showAddCompany" class="nested-form">
          <div class="nested-grid">
            <input v-model="newCompany.companyName" placeholder="Company Name" />
            <input v-model="newCompany.industry" placeholder="Industry" />
            <input v-model="newCompany.location" placeholder="Location" />
            <button class="primary-btn" @click="createCompany">
              Save Company
            </button>
          </div>
        </div>

        <div class="form-group">
          <label>Title <span class="required">*</span></label>
          <input v-model="newJob.title" placeholder="e.g. Senior Backend Engineer" :class="{ 'is-invalid': errors.title }" />
          <span class="error-msg" v-if="errors.title">{{ errors.title }}</span>
        </div>

        <div class="form-group full-width">
          <label>Description <span class="required">*</span></label>
          <textarea v-model="newJob.description" placeholder="Detailed job description..." :class="{ 'is-invalid': errors.description }"></textarea>
          <span class="error-msg" v-if="errors.description">{{ errors.description }}</span>
        </div>

        <div class="form-group">
          <label>Required Skills <span class="required">*</span></label>
          <input v-model="newJob.requiredSkills" placeholder="e.g. Java, Vue.js, SQL" :class="{ 'is-invalid': errors.requiredSkills }"/>
          <span class="error-msg" v-if="errors.requiredSkills">{{ errors.requiredSkills }}</span>
        </div>

        <!-- 🔥 Job Type Dropdown -->
        <div class="form-group">
          <label>Job Type <span class="required">*</span></label>
          <select v-model="newJob.jobType" :class="{ 'is-invalid': errors.jobType }">
            <option value="">Select Job Type</option>
            <option>Full-Time</option>
            <option>Part-Time</option>
            <option>Contract</option>
            <option>Hybrid</option>
            <option>Remote</option>
          </select>
          <span class="error-msg" v-if="errors.jobType">{{ errors.jobType }}</span>
        </div>

        <div class="form-group">
          <label>Salary Range <span class="required">*</span></label>
          <div class="salary-input-wrapper">
            <input v-model="newJob.salaryRange" placeholder="e.g. 5-8" :class="{ 'is-invalid': errors.salaryRange }" @blur="formatSalary" />
            <span class="currency-badge">LPA</span>
          </div>
          <span class="error-msg" v-if="errors.salaryRange">{{ errors.salaryRange }}</span>
        </div>

      </div>

      <div class="form-footer">
        <button class="secondary-btn" @click="toggleAddJob" style="margin-right: 15px;">
          Cancel
        </button>
        <button class="primary-btn submit-btn" @click="createJob" :disabled="loading">
          <i class="bi bi-check2-circle me-2" v-if="!loading"></i>
          <span class="spinner-border spinner-border-sm me-2" v-if="loading"></span>
          {{ loading ? "Saving Job..." : "Save Job Entry" }}
        </button>
      </div>
    </div>

    <!-- UPDATE JOB SECTION -->
    <div v-if="showUpdateJob" class="card-section update-section">
      <h3 class="section-title">Update Job</h3>

      <div class="form-grid">

        <!-- Company -->
        <div class="form-group">
          <label>Company <span class="required">*</span></label>
          <div class="company-row">
            <select v-model.number="editingJob.companyId" :class="{ 'is-invalid': updateErrors.companyId }">
              <option :value="null">Select Company</option>
              <option v-for="c in companies" :key="c.companyId" :value="c.companyId">
                {{ c.companyName }}
              </option>
            </select>
          </div>
          <span class="error-msg" v-if="updateErrors.companyId">{{ updateErrors.companyId }}</span>
        </div>

        <div class="form-group">
          <label>Title <span class="required">*</span></label>
          <input v-model="editingJob.title" placeholder="e.g. Senior Backend Engineer" :class="{ 'is-invalid': updateErrors.title }" />
          <span class="error-msg" v-if="updateErrors.title">{{ updateErrors.title }}</span>
        </div>

        <div class="form-group full-width">
          <label>Description <span class="required">*</span></label>
          <textarea v-model="editingJob.description" placeholder="Detailed job description..." :class="{ 'is-invalid': updateErrors.description }"></textarea>
          <span class="error-msg" v-if="updateErrors.description">{{ updateErrors.description }}</span>
        </div>

        <div class="form-group">
          <label>Required Skills <span class="required">*</span></label>
          <input v-model="editingJob.requiredSkills" placeholder="e.g. Java, Vue.js, SQL" :class="{ 'is-invalid': updateErrors.requiredSkills }"/>
          <span class="error-msg" v-if="updateErrors.requiredSkills">{{ updateErrors.requiredSkills }}</span>
        </div>

        <!-- 🔥 Job Type Dropdown -->
        <div class="form-group">
          <label>Job Type <span class="required">*</span></label>
          <select v-model="editingJob.jobType" :class="{ 'is-invalid': updateErrors.jobType }">
            <option value="">Select Job Type</option>
            <option>Full-Time</option>
            <option>Part-Time</option>
            <option>Contract</option>
            <option>Hybrid</option>
            <option>Remote</option>
          </select>
          <span class="error-msg" v-if="updateErrors.jobType">{{ updateErrors.jobType }}</span>
        </div>

        <div class="form-group">
          <label>Salary Range <span class="required">*</span></label>
          <div class="salary-input-wrapper">
            <input v-model="editingJob.salaryRange" placeholder="e.g. 5-8" :class="{ 'is-invalid': updateErrors.salaryRange }" @blur="formatUpdateSalary" />
            <span class="currency-badge">LPA</span>
          </div>
          <span class="error-msg" v-if="updateErrors.salaryRange">{{ updateErrors.salaryRange }}</span>
        </div>

      </div>

      <div class="form-footer">
        <button class="secondary-btn" @click="cancelUpdate" style="margin-right: 15px;">
          Cancel
        </button>
        <button class="primary-btn submit-btn" @click="saveUpdateJob" :disabled="loading">
          <i class="bi bi-pencil-square me-2" v-if="!loading"></i>
          <span class="spinner-border spinner-border-sm me-2" v-if="loading"></span>
          {{ loading ? "Updating..." : "Update Job" }}
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
              <th>Actions</th>
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
              <td>
                <button class="action-btn edit-btn" @click="editJob(job)">
                  <i class="bi bi-pencil-fill"></i> Edit
                </button>
              </td>
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
  updateJob,
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

    const showUpdateJob = ref(false)
    const editingJob = ref({})
    const updateErrors = ref({})

    const errors = ref({})

    const validateJobData = (targetJob, targetErrors) => {
      targetErrors.value = {}
      let isValid = true

      if (!targetJob.value.companyId) { targetErrors.value.companyId = "Company is required"; isValid = false }
      if (!targetJob.value.title?.trim()) { targetErrors.value.title = "Job title is required"; isValid = false }
      if (!targetJob.value.description?.trim()) { targetErrors.value.description = "Description is required"; isValid = false }
      if (!targetJob.value.requiredSkills?.trim()) { targetErrors.value.requiredSkills = "Required skills are needed"; isValid = false }
      if (!targetJob.value.jobType) { targetErrors.value.jobType = "Select a job type"; isValid = false }
      if (!targetJob.value.salaryRange?.trim()) { targetErrors.value.salaryRange = "Salary range is required"; isValid = false }

      return isValid
    }

    const validateJob = () => validateJobData(newJob, errors)
    const validateUpdate = () => validateJobData(editingJob, updateErrors)

    const appendLPA = (targetJob) => {
      let val = targetJob.value.salaryRange?.trim() || ""
      if (val && !/LPA$/i.test(val)) {
        targetJob.value.salaryRange = val + " LPA"
      }
    }

    const formatSalary = () => appendLPA(newJob)
    const formatUpdateSalary = () => appendLPA(editingJob)

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
      if (showAddJob.value) showUpdateJob.value = false
    }

    const toggleAddCompany = () => {
      showAddCompany.value = !showAddCompany.value
    }

    const editJob = (job) => {
      editingJob.value = { ...job }
      // Clean up string fields in case of null
      editingJob.value.title = editingJob.value.title || ""
      editingJob.value.description = editingJob.value.description || ""
      editingJob.value.requiredSkills = editingJob.value.requiredSkills || ""
      editingJob.value.jobType = editingJob.value.jobType || ""
      editingJob.value.salaryRange = editingJob.value.salaryRange || ""
      
      let val = editingJob.value.salaryRange
      if (val && /LPA$/i.test(val)) {
        editingJob.value.salaryRange = val.replace(/\s*LPA$/i, "")
      }

      showUpdateJob.value = true
      showAddJob.value = false
      updateErrors.value = {}
    }

    const cancelUpdate = () => {
      showUpdateJob.value = false
      editingJob.value = {}
      updateErrors.value = {}
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
      formatSalary()
      if (!validateJob()) {
        return
      }

      try {
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
        errors.value = {}

        showAddJob.value = false
        await loadJobs()
      } catch (err) {
        alert("Error: " + err.message)
      } finally {
        loading.value = false
      }
    }

    const saveUpdateJob = async () => {
      formatUpdateSalary()
      if (!validateUpdate()) {
        return
      }

      try {
        loading.value = true
        await updateJob(editingJob.value.jobId, editingJob.value)

        alert("Job Updated Successfully")
        showUpdateJob.value = false
        await loadJobs()
      } catch (err) {
        alert("Error updating job: " + err.message)
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
      errors,
      updateErrors,
      showAddJob,
      showAddCompany,
      showUpdateJob,
      newJob,
      newCompany,
      editingJob,
      formatSalary,
      formatUpdateSalary,
      toggleAddJob,
      toggleAddCompany,
      editJob,
      cancelUpdate,
      createJob,
      saveUpdateJob,
      createCompany
    }
  }
}
</script>

<style scoped>

.jobs-page {
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

/* Buttons */
.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.primary-btn:hover {
  background: #1d4ed8;
  transform: translateY(-1px);
}

.primary-btn:disabled {
  background: #94a3b8;
  cursor: not-allowed;
  transform: none;
}

.secondary-btn {
  background: var(--recent-bg);
  color: var(--text-primary);
  border: 1px solid var(--border);
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
}

.secondary-btn:hover {
  background: var(--border);
  color: var(--text-primary);
}

.company-row .secondary-btn {
  padding: 8px 14px;
  margin-left: 10px;
}

.add-job-btn {
  font-size: 15px;
  padding: 10px 24px;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
}

/* Cards */
.card-section {
  background: var(--bg-card);
  padding: 30px;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.05);
  margin-bottom: 25px;
  border-top: 4px solid #3b82f6; 
}

.update-section {
  border-top: 4px solid #f59e0b; /* Distinct color for update */
}

.section-title {
  margin-bottom: 20px;
  font-weight: 600;
  color: var(--text-primary);
}
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
  color: var(--text-muted);
}

input, textarea, select {
  width: 100%;
  padding: 8px;
  margin-top: 6px;
  border-radius: 8px;
  border: 1px solid var(--border);
  background: var(--bg-card);
  color: var(--text-primary);
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
  background: var(--recent-bg);
  padding: 15px;
  border-radius: 10px;
  grid-column: 1 / -1;
  border: 1px solid var(--border);
}

.nested-grid {
  display: flex;
  gap: 10px;
  align-items: center;
  flex-wrap: wrap;
}

.nested-grid input {
  flex: 1;
  min-width: 150px;
  margin-top: 0;
  background: var(--bg-card);
  color: var(--text-primary);
  border: 1px solid var(--border);
}

.nested-grid button {
  white-space: nowrap;
}

/* Validation and Form Styling */
.required {
  color: #ef4444;
}

.is-invalid {
  border-color: #ef4444 !important;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.error-msg {
  color: #ef4444;
  font-size: 13px;
  margin-top: 6px;
  display: block;
}

.salary-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.salary-input-wrapper input {
  padding-right: 50px;
}

.currency-badge {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #64748b;
  font-weight: 500;
  font-size: 13px;
  pointer-events: none;
  margin-top: 3px;
}

.submit-btn {
  padding: 10px 24px;
  font-size: 15px;
}

input:focus, textarea:focus, select:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
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
  background: var(--recent-bg);
  text-align: left;
  padding: 12px;
  font-weight: 600;
  font-size: 14px;
  color: var(--text-muted);
  border-bottom: 2px solid var(--recent-border);
}

.job-table td {
  padding: 12px;
  border-bottom: 1px solid var(--border);
  font-size: 14px;
  color: var(--text-primary);
}

.job-table tbody tr:hover {
  background: var(--recent-bg);
}

.action-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  border-radius: 6px;
  padding: 6px 12px;
  transition: all 0.2s;
}

.edit-btn {
  color: #0284c7;
  background: #e0f2fe;
}

.edit-btn:hover {
  background: #bae6fd;
}

.update-section {
  border-top: 4px solid #f59e0b;
}

.fw {
  font-weight: 600;
  color: var(--text-primary);
}

.no-data {
  text-align: center;
  padding: 20px;
  color: var(--text-muted);
}

</style>