<template>
  <div class="jobs-page">

    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Your Job Postings</h2>
        <p class="text-muted small ms-3 mb-0">Manage and track your company's job listings.</p>
      </div>
      <div class="header-right">
        <button class="primary-btn add-job-btn" @click="toggleAddJob" v-if="!showUpdateJob">
          <i class="bi bi-plus-circle-fill me-2"></i>
          {{ showAddJob ? "Cancel" : "Post New Job" }}
        </button>
        <button class="secondary-btn add-job-btn" @click="cancelUpdate" v-else>
          <i class="bi bi-x-circle-fill me-2"></i> Cancel Update
        </button>
      </div>
    </div>

    <!-- ADD JOB SECTION -->
    <div v-if="showAddJob" class="card-section">
      <h3 class="section-title">Post a New Position</h3>

      <div class="form-grid">
        <div class="form-group">
          <label>Job Title <span class="required">*</span></label>
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

        <div class="form-group">
          <label>Last Date to Apply</label>
          <input type="date" v-model="newJob.lastDate" />
        </div>
      </div>

      <div class="form-footer">
        <button class="secondary-btn me-2" @click="toggleAddJob">Cancel</button>
        <button class="primary-btn submit-btn" @click="createJob" :disabled="loading">
          <span class="spinner-border spinner-border-sm me-2" v-if="loading"></span>
          {{ loading ? "Posting..." : "Post Job" }}
        </button>
      </div>
    </div>

    <!-- UPDATE JOB SECTION -->
    <div v-if="showUpdateJob" class="card-section update-section">
      <h3 class="section-title">Update Job Posting</h3>

      <div class="form-grid">
        <div class="form-group">
          <label>Job Title <span class="required">*</span></label>
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

        <div class="form-group">
          <label>Last Date to Apply</label>
          <input type="date" v-model="editingJob.lastDate" />
        </div>
      </div>

      <div class="form-footer">
        <button class="secondary-btn me-2" @click="cancelUpdate">Cancel</button>
        <button class="primary-btn submit-btn" @click="saveUpdateJob" :disabled="loading">
          <span class="spinner-border spinner-border-sm me-2" v-if="loading"></span>
          {{ loading ? "Updating..." : "Update Posting" }}
        </button>
      </div>
    </div>

    <!-- JOB TABLE -->
    <div class="card-section listing-section">
      <h3 class="section-title">Current Opportunities</h3>

      <div class="table-wrapper">
        <table class="job-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Type</th>
              <th>Salary</th>
               <th>Deadline</th>
               <th>Status</th>
               <th>Skills</th>
               <th>Actions</th>
             </tr>
           </thead>
          <tbody>
            <tr v-for="job in jobs" :key="job.jobId">
              <td class="fw-semibold">{{ job.title }}</td>
              <td><span class="badge bg-light text-dark">{{ job.jobType }}</span></td>
              <td>{{ job.salaryRange }}</td>
               <td>{{ job.lastDate ? job.lastDate.split('T')[0] : 'Open' }}</td>
               <td>
                 <span v-if="job.isApproved" class="badge bg-success-soft text-success">Approved</span>
                 <span v-else class="badge bg-warning-soft text-warning">Pending Approval</span>
               </td>
               <td>{{ job.requiredSkills }}</td>
              <td>
                <button class="action-btn edit-btn" @click="editJob(job)">
                  <i class="bi bi-pencil-fill me-1"></i> Edit
                </button>
              </td>
            </tr>
            <tr v-if="jobs.length === 0">
              <td colspan="6" class="no-data py-5">
                <div class="text-center">
                  <p class="mb-0">You haven't posted any jobs yet.</p>
                  <button class="btn btn-link" @click="toggleAddJob">Post your first job</button>
                </div>
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
import {
  getCompanyJobs,
  addCompanyJob,
  updateCompanyJob
} from "@/services/api"
import { useNotification } from "@/composables/useNotification"

const { notify } = useNotification()

const jobs = ref([])
const loading = ref(false)
const showAddJob = ref(false)
const showUpdateJob = ref(false)

const newJob = ref({
  title: "",
  description: "",
  requiredSkills: "",
  jobType: "",
  salaryRange: "",
  lastDate: ""
})

const editingJob = ref({})
const errors = ref({})
const updateErrors = ref({})

const validateJobData = (targetJob, targetErrors) => {
  targetErrors.value = {}
  let isValid = true

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

const loadJobs = async () => {
  try {
    jobs.value = await getCompanyJobs()
  } catch (err) {
    console.error("Error loading jobs:", err)
  }
}

const toggleAddJob = () => {
  showAddJob.value = !showAddJob.value
  if (showAddJob.value) showUpdateJob.value = false
}

const editJob = (job) => {
  editingJob.value = { ...job }
  editingJob.value.lastDate = editingJob.value.lastDate ? editingJob.value.lastDate.split('T')[0] : ""
  
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

const createJob = async () => {
  formatSalary()
  if (!validateJob()) return

  try {
    loading.value = true
    const payload = { ...newJob.value }
    if (!payload.lastDate) payload.lastDate = null

    await addCompanyJob(payload)
    notify("Job Posted Successfully!", "success")

    newJob.value = { title: "", description: "", requiredSkills: "", jobType: "", salaryRange: "", lastDate: "" }
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
  if (!validateUpdate()) return

  try {
    loading.value = true
    const payload = { ...editingJob.value }
    if (!payload.lastDate) payload.lastDate = null

    await updateCompanyJob(editingJob.value.jobId, payload)
    notify("Listing Updated Successfully!", "success")
    showUpdateJob.value = false
    await loadJobs()
  } catch (err) {
    notify("Error: " + err.message, "error")
  } finally {
    loading.value = false
  }
}

onMounted(loadJobs)
</script>

<style scoped>
.jobs-page {
  padding: 1rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  background: white;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.primary-btn {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 0.75rem;
  font-weight: 600;
  transition: all 0.3s;
}

.primary-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(37, 99, 235, 0.4);
}

.secondary-btn {
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #e2e8f0;
  padding: 0.6rem 1.2rem;
  border-radius: 0.75rem;
  font-weight: 600;
  transition: all 0.3s;
}

.card-section {
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
  border-top: 5px solid #3b82f6;
}

.update-section {
  border-top-color: #f59e0b;
}

.listing-section {
    border-top-color: #10b981;
}

.section-title {
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 0.5rem;
}

input, textarea, select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  transition: all 0.2s;
}

input:focus, textarea:focus, select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.salary-input-wrapper {
  position: relative;
}

.currency-badge {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-weight: 700;
  font-size: 0.8rem;
}

.job-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0 0.5rem;
}

.job-table th {
  padding: 1rem;
  background: #f8fafc;
  color: #64748b;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.05em;
}

.job-table td {
  padding: 1rem;
  border-bottom: 1px solid #f1f5f9;
}

.action-btn {
  border: none;
  padding: 0.4rem 0.8rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.edit-btn {
  background: #eff6ff;
  color: #2563eb;
}

.edit-btn:hover {
  background: #dbeafe;
}

.bg-success-soft {
  background: rgba(16, 185, 129, 0.1);
}

.bg-warning-soft {
  background: rgba(245, 158, 11, 0.1);
}

.required {
  color: #ef4444;
}

.is-invalid {
  border-color: #ef4444;
}

.error-msg {
  color: #ef4444;
  font-size: 0.75rem;
  margin-top: 0.25rem;
}

.no-data {
  color: #94a3b8;
}
</style>
