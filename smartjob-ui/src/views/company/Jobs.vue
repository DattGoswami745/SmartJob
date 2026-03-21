<template>
  <div class="jobs-page">

    <!-- HEADER -->
    <div class="page-header">
      <div class="header-left">
        <h2>Your Job Postings</h2>
        <p class="text-muted small ms-3 mb-0">Manage and track your company's job listings.</p>
      </div>
      <div class="header-right">
        <button 
          class="primary-btn add-job-btn" 
          @click="toggleAddJob" 
          v-if="!showUpdateJob"
          :disabled="!isVerified"
          :title="!isVerified ? 'Please verify your company to post jobs' : ''"
        >
          <i class="bi bi-plus-circle-fill me-2"></i>
          {{ showAddJob ? "Cancel" : "Post New Job" }}
        </button>
        <button class="secondary-btn add-job-btn" @click="cancelUpdate" v-else>
          <i class="bi bi-x-circle-fill me-2"></i> Cancel Update
        </button>
      </div>
    </div>

    <!-- VERIFICATION WARNING -->
    <div v-if="!isVerified && !loading" class="alert alert-warning border-0 shadow-sm rounded-4 p-4 mb-4 d-flex align-items-center justify-content-between">
      <div class="d-flex align-items-center">
        <div class="warning-icon me-3 bg-warning bg-opacity-10 p-3 rounded-circle text-warning">
          <i class="bi bi-shield-lock-fill fs-4"></i>
        </div>
        <div>
          <h5 class="fw-bold mb-1">Company Verification Required</h5>
          <p class="mb-0 text-muted">You must complete your company verification before you can post new jobs or recruitment actions.</p>
        </div>
      </div>
      <router-link to="/company/verification" class="btn btn-warning fw-bold px-4 rounded-pill">
        Verify Now
      </router-link>
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
          <label>Job Description <span class="required">*</span></label>
          <div class="description-type-toggle mb-3">
            <button 
              type="button" 
              class="toggle-btn" 
              :class="{ active: newJob.descriptionType === 'text' }"
              @click="newJob.descriptionType = 'text'"
            >
              <i class="bi bi-fonts me-2"></i> Write Description
            </button>
            <button 
              type="button" 
              class="toggle-btn" 
              :class="{ active: newJob.descriptionType === 'file' }"
              @click="newJob.descriptionType = 'file'"
            >
              <i class="bi bi-file-earmark-arrow-up me-2"></i> Upload Document
            </button>
          </div>

          <!-- Text Description -->
          <div v-if="newJob.descriptionType === 'text'">
            <textarea 
              v-model="newJob.jobDescriptionText" 
              placeholder="Detailed job description (supports basic formatting)..." 
              class="rich-textarea"
              :class="{ 'is-invalid': errors.jobDescriptionText }"
            ></textarea>
            <div class="char-count small text-muted text-end mt-1">{{ (newJob.jobDescriptionText || '').length }} characters</div>
            <span class="error-msg" v-if="errors.jobDescriptionText">{{ errors.jobDescriptionText }}</span>
          </div>

          <!-- File Upload -->
          <div v-else class="file-upload-zone" :class="{ 'has-file': newJob.descriptionFile }">
            <input 
              type="file" 
              id="newJobFile" 
              class="d-none" 
              @change="handleNewFileChange" 
              accept=".pdf,.doc,.docx" 
            />
            <label for="newJobFile" class="file-label">
              <div v-if="!newJob.descriptionFile">
                <i class="bi bi-cloud-upload fs-2 d-block mb-2"></i>
                <span>Click to upload PDF, DOC, or DOCX</span>
                <p class="small text-muted mb-0">Max size: 5MB</p>
              </div>
              <div v-else class="file-info">
                <i class="bi bi-file-earmark-check-fill text-primary fs-3 me-3"></i>
                <div class="text-start">
                  <div class="fw-bold">{{ newJob.descriptionFile.name }}</div>
                  <div class="small text-muted">{{ (newJob.descriptionFile.size / 1024).toFixed(1) }} KB</div>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger ms-auto" @click.stop.prevent="removeNewFile">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </label>
            <span class="error-msg" v-if="errors.descriptionFile">{{ errors.descriptionFile }}</span>
          </div>
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
          <label>Job Description <span class="required">*</span></label>
          <div class="description-type-toggle mb-3">
            <button 
              type="button" 
              class="toggle-btn" 
              :class="{ active: editingJob.descriptionType === 'text' }"
              @click="editingJob.descriptionType = 'text'"
            >
              <i class="bi bi-fonts me-2"></i> Write Description
            </button>
            <button 
              type="button" 
              class="toggle-btn" 
              :class="{ active: editingJob.descriptionType === 'file' }"
              @click="editingJob.descriptionType = 'file'"
            >
              <i class="bi bi-file-earmark-arrow-up me-2"></i> Upload Document
            </button>
          </div>

          <!-- Text Description -->
          <div v-if="editingJob.descriptionType === 'text'">
            <textarea 
              v-model="editingJob.jobDescriptionText" 
              placeholder="Detailed job description (supports basic formatting)..." 
              class="rich-textarea"
              :class="{ 'is-invalid': updateErrors.jobDescriptionText }"
            ></textarea>
            <div class="char-count small text-muted text-end mt-1">{{ (editingJob.jobDescriptionText || '').length }} characters</div>
            <span class="error-msg" v-if="updateErrors.jobDescriptionText">{{ updateErrors.jobDescriptionText }}</span>
          </div>

          <!-- File Upload -->
          <div v-else class="file-upload-zone" :class="{ 'has-file': editingJob.descriptionFile || editingJob.jobDescriptionFile }">
            <input 
              type="file" 
              id="editJobFile" 
              class="d-none" 
              @change="handleEditFileChange" 
              accept=".pdf,.doc,.docx" 
            />
            <label for="editJobFile" class="file-label">
              <div v-if="!editingJob.descriptionFile && !editingJob.jobDescriptionFile">
                <i class="bi bi-cloud-upload fs-2 d-block mb-2"></i>
                <span>Click to upload PDF, DOC, or DOCX</span>
                <p class="small text-muted mb-0">Max size: 5MB</p>
              </div>
              <div v-else class="file-info">
                <i class="bi bi-file-earmark-check-fill text-primary fs-3 me-3"></i>
                <div class="text-start">
                  <div class="fw-bold">{{ editingJob.descriptionFile ? editingJob.descriptionFile.name : (editingJob.jobDescriptionFile || '').split('/').pop() }}</div>
                  <div class="small text-muted" v-if="editingJob.descriptionFile">{{ (editingJob.descriptionFile.size / 1024).toFixed(1) }} KB</div>
                  <div class="small text-primary" v-else>Existing document</div>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger ms-auto" @click.stop.prevent="removeEditFile">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </label>
            <span class="error-msg" v-if="updateErrors.descriptionFile">{{ updateErrors.descriptionFile }}</span>
          </div>
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
               <td>
                 <div class="d-flex flex-wrap gap-1">
                   <span 
                     v-for="(skill, index) in (job.requiredSkills || '').split(',')" 
                     :key="index"
                     class="badge rounded-pill skill-tag"
                     :class="'tag-color-' + (index % 5)"
                   >
                     {{ skill.trim() }}
                   </span>
                 </div>
               </td>
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
  updateCompanyJob,
  checkSession,
  getSetupCompanies
} from "@/services/api"
import { useNotification } from "@/composables/useNotification"

const { notify } = useNotification()

const jobs = ref([])
const loading = ref(false)
const showAddJob = ref(false)
const showUpdateJob = ref(false)
const isVerified = ref(true) // Default to true until checked

const fetchVerificationStatus = async () => {
  try {
    const userData = await checkSession();
    const companyId = userData.companyId;
    if (companyId) {
      const companies = await getSetupCompanies();
      const current = companies.find(c => c.companyId === companyId);
      isVerified.value = current?.isCompanyVerified || false;
    }
  } catch (err) {
    console.error("Verification check failed", err);
  }
}

const newJob = ref({
  title: "",
  description: "", // Keep for basic overview if needed, but primarily using rich text now
  requiredSkills: "",
  jobType: "",
  salaryRange: "",
  lastDate: "",
  descriptionType: "text",
  jobDescriptionText: "",
  descriptionFile: null
})

const editingJob = ref({})
const errors = ref({})
const updateErrors = ref({})

const validateJobData = (targetJob, targetErrors) => {
  targetErrors.value = {}
  let isValid = true

  if (!targetJob.value.title?.trim()) { targetErrors.value.title = "Job title is required"; isValid = false }
  
  if (targetJob.value.descriptionType === 'text') {
    if (!targetJob.value.jobDescriptionText?.trim()) { 
      targetErrors.value.jobDescriptionText = "Job description text is required"; 
      isValid = false 
    }
  } else {
    if (!targetJob.value.descriptionFile && !targetJob.value.jobDescriptionFile) {
      targetErrors.value.descriptionFile = "Please upload a description document";
      isValid = false
    }
  }

  if (!targetJob.value.requiredSkills?.trim()) { targetErrors.value.requiredSkills = "Required skills are needed"; isValid = false }
  if (!targetJob.value.jobType) { targetErrors.value.jobType = "Select a job type"; isValid = false }
  if (!targetJob.value.salaryRange?.trim()) { targetErrors.value.salaryRange = "Salary range is required"; isValid = false }

  return isValid
}

const handleNewFileChange = (e) => {
  const file = e.target.files[0]
  if (file) newJob.value.descriptionFile = file
}
const removeNewFile = () => { newJob.value.descriptionFile = null }

const handleEditFileChange = (e) => {
  const file = e.target.files[0]
  if (file) editingJob.value.descriptionFile = file
}
const removeEditFile = () => {
  editingJob.value.descriptionFile = null
  editingJob.value.jobDescriptionFile = null
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
  
  // Set description type based on existing data
  editingJob.value.descriptionType = job.jobDescriptionFile ? 'file' : 'text'
  editingJob.value.descriptionFile = null // Reset new file selection

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
    
    const formData = new FormData()
    formData.append("title", newJob.value.title)
    formData.append("description", newJob.value.title) // Use title as short desc for now
    formData.append("requiredSkills", newJob.value.requiredSkills)
    formData.append("jobType", newJob.value.jobType)
    formData.append("salaryRange", newJob.value.salaryRange)
    if (newJob.value.lastDate) formData.append("lastDate", newJob.value.lastDate)
    
    if (newJob.value.descriptionType === 'text') {
      formData.append("jobDescriptionText", newJob.value.jobDescriptionText)
    } else if (newJob.value.descriptionFile) {
      formData.append("descriptionFile", newJob.value.descriptionFile)
    }

    await addCompanyJob(formData)
    notify("Job Posted Successfully!", "success")

    newJob.value = { 
      title: "", description: "", requiredSkills: "", jobType: "", 
      salaryRange: "", lastDate: "", descriptionType: "text", 
      jobDescriptionText: "", descriptionFile: null 
    }
    errors.value = {}
    showAddJob.value = false
    await loadJobs()
  } catch (err) {
    notify("Error: " + err.message, "error")
  } finally {
    loading.value = false
  }
}

const saveUpdateJob = async () => {
  formatUpdateSalary()
  if (!validateUpdate()) return

  try {
    loading.value = true
    const formData = new FormData()
    formData.append("jobId", editingJob.value.jobId)
    formData.append("title", editingJob.value.title)
    formData.append("description", editingJob.value.title)
    formData.append("requiredSkills", editingJob.value.requiredSkills)
    formData.append("jobType", editingJob.value.jobType)
    formData.append("salaryRange", editingJob.value.salaryRange)
    if (editingJob.value.lastDate) formData.append("lastDate", editingJob.value.lastDate)

    if (editingJob.value.descriptionType === 'text') {
      formData.append("jobDescriptionText", editingJob.value.jobDescriptionText || "")
      formData.append("jobDescriptionFile", "") // Explicitly clear file path if switched to text
    } else {
      if (editingJob.value.descriptionFile) {
        formData.append("descriptionFile", editingJob.value.descriptionFile)
      } else {
        formData.append("jobDescriptionFile", editingJob.value.jobDescriptionFile || "")
      }
    }

    await updateCompanyJob(editingJob.value.jobId, formData)
    notify("Listing Updated Successfully!", "success")
    showUpdateJob.value = false
    await loadJobs()
  } catch (err) {
    notify("Error: " + err.message, "error")
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await fetchVerificationStatus();
  await loadJobs();
})
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
  background: #f0f9ff;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.edit-btn:hover {
  background: #0ea5e9;
  color: white;
  border-color: #0ea5e9;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.2);
}

/* Job Description Styles */
.description-type-toggle {
  display: flex;
  gap: 1rem;
}

.toggle-btn {
  flex: 1;
  padding: 0.75rem;
  border: 2px solid #e2e8f0;
  background: white;
  border-radius: 0.75rem;
  font-weight: 600;
  color: #64748b;
  transition: all 0.2s;
}

.toggle-btn:hover {
  border-color: #3b82f6;
  background: #f8fafc;
}

.toggle-btn.active {
  background: #3b82f6;
  color: white;
  border-color: #3b82f6;
  box-shadow: 0 4px 6px -1px rgba(59, 130, 246, 0.3);
}

.rich-textarea {
  min-height: 250px;
  font-family: 'Inter', sans-serif;
  line-height: 1.6;
}

.file-upload-zone {
  border: 2px dashed #e2e8f0;
  border-radius: 1rem;
  padding: 2.5rem;
  text-align: center;
  transition: all 0.3s;
  cursor: pointer;
  background: #f8fafc;
}

.file-upload-zone:hover {
  border-color: #3b82f6;
  background: #eff6ff;
}

.file-upload-zone.has-file {
  border-color: #10b981;
  border-style: solid;
  background: #f0fdf4;
}

.file-label {
  width: 100%;
  height: 100%;
  cursor: pointer;
  display: block;
}

.file-info {
  display: flex;
  align-items: center;
  padding: 0.5rem;
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

/* Colorful Skill Tags */
.skill-tag {
  padding: 4px 10px;
  font-weight: 600;
  font-size: 0.7rem;
  border: 1px solid transparent;
}
.tag-color-0 { background: rgba(59, 130, 246, 0.1); color: #2563eb; border-color: rgba(59, 130, 246, 0.2); }
.tag-color-1 { background: rgba(16, 185, 129, 0.1); color: #059669; border-color: rgba(16, 185, 129, 0.2); }
.tag-color-2 { background: rgba(245, 158, 11, 0.1); color: #d97706; border-color: rgba(245, 158, 11, 0.2); }
.tag-color-3 { background: rgba(139, 92, 246, 0.1); color: #7c3aed; border-color: rgba(139, 92, 246, 0.2); }
.tag-color-4 { background: rgba(236, 72, 153, 0.1); color: #db2777; border-color: rgba(236, 72, 153, 0.2); }
</style>
