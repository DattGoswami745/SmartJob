<template>
  <div class="report-management-page">
    <div class="page-header mb-4">
      <div class="header-left">
        <h2 class="fw-bold"><i class="bi bi-gear-fill me-2"></i>Report Management</h2>
        <p class="text-muted mb-0">Configure and manage dynamic system reports</p>
      </div>
      <div class="header-right">
        <button class="btn btn-primary d-flex align-items-center gap-2" @click="resetForm" v-if="editingReport">
          <i class="bi bi-plus-lg"></i> Create New Report
        </button>
      </div>
    </div>

    <div class="row">
      <!-- REPORT CONFIGURATION FORM -->
      <div class="col-lg-5">
        <div class="card shadow-sm border-0 mb-4">
          <div class="card-header bg-white py-3">
            <h5 class="mb-0 fw-bold">{{ editingReport ? 'Edit' : 'Create' }} Report Configuration</h5>
          </div>
          <div class="card-body">
            <form @submit.prevent="saveConfiguration">
              <div class="mb-3">
                <label class="form-label fw-semibold">Report Name</label>
                <input v-model="form.reportName" type="text" class="form-control" placeholder="e.g., Monthly Applicant Summary" required />
              </div>
              
              <div class="mb-3">
                <label class="form-label fw-semibold">Description</label>
                <textarea v-model="form.description" class="form-control" rows="2" placeholder="Briefly describe what this report covers..."></textarea>
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold">Base Data Source</label>
                <select v-model="form.baseTable" class="form-select" @change="onTableChange" required>
                  <option value="" disabled>Select a table...</option>
                  <option v-for="table in availableTables" :key="table.id" :value="table.id">
                    {{ table.label }}
                  </option>
                </select>
              </div>

              <div class="mb-3" v-if="form.baseTable">
                <label class="form-label fw-semibold">Select Fields to Include</label>
                <div class="fields-grid p-2 border rounded">
                  <div v-for="field in currentFields" :key="field.id" class="form-check">
                    <input 
                      class="form-check-input" 
                      type="checkbox" 
                      :id="'field-' + field.id"
                      :value="field"
                      v-model="selectedFields"
                    />
                    <label class="form-check-label" :for="'field-' + field.id">
                      {{ field.label }}
                    </label>
                  </div>
                </div>
                <small class="text-muted">{{ selectedFields.length }} fields selected</small>
              </div>

              <div class="d-flex gap-2 mt-4">
                <button type="submit" class="btn btn-success flex-grow-1 py-2 fw-bold" :disabled="loading">
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                  {{ editingReport ? 'Update Configuration' : 'Save Configuration' }}
                </button>
                <button type="button" class="btn btn-outline-secondary px-4" @click="resetForm">Cancel</button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- EXISTING REPORTS LIST -->
      <div class="col-lg-7">
        <div class="card shadow-sm border-0">
          <div class="card-header bg-white py-3">
            <h5 class="mb-0 fw-bold">Configured Reports</h5>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-hover align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Report Name</th>
                    <th>Source</th>
                    <th>Fields</th>
                    <th>Created At</th>
                    <th class="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="report in reports" :key="report.reportId">
                    <td class="fw-bold">{{ report.reportName }}</td>
                    <td><span class="badge bg-secondary">{{ getSourceLabel(report.baseTable) }}</span></td>
                    <td>{{ parseFields(report.selectedFields).length }} fields</td>
                    <td class="small">{{ new Date(report.createdAt).toLocaleDateString() }}</td>
                    <td class="text-end">
                      <button class="btn btn-sm btn-outline-primary me-2" @click="editReport(report)">
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-info me-2" @click="viewReport(report)">
                        <i class="bi bi-eye"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" @click="confirmDelete(report)">
                        <i class="bi bi-trash"></i>
                      </button>
                    </td>
                  </tr>
                  <tr v-if="reports.length === 0">
                    <td colspan="5" class="text-center py-5 text-muted">No report configurations found.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue"
import { useRouter } from "vue-router"
import { getReportConfigs, createReportConfig, updateReportConfig, deleteReportConfig } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"

const router = useRouter()
const reports = ref([])
const loading = ref(false)
const editingReport = ref(null)

const form = ref({
  reportName: "",
  description: "",
  baseTable: "",
  selectedFields: "[]",
  filters: "[]"
})

const selectedFields = ref([])

const availableTables = [
  { 
    id: "(SELECT j.JobId, j.Title AS JobTitle, c.CompanyName AS CompanyName, j.JobType AS JobType, j.SalaryRange AS SalaryRange, j.PostedDate AS PostedDate, j.CompanyId AS CompanyId FROM Jobs j LEFT JOIN Companies c ON j.CompanyId = c.CompanyId) AS JobReport", 
    label: "Jobs" 
  },
  { 
    id: "(SELECT a.ApplicationId, u.FullName AS UserName, u.Email AS UserEmail, j.Title AS JobTitle, c.CompanyName AS CompanyName, a.AppliedDate AS AppliedDate, a.ApplicationStatus AS Status, j.CompanyId AS CompanyId FROM Applications a JOIN Users u ON a.UserId = u.UserId JOIN Jobs j ON a.JobId = j.JobId LEFT JOIN Companies c ON j.CompanyId = c.CompanyId) AS AppReport", 
    label: "Applications" 
  },
  { id: "Users", label: "User Accounts" },
  { id: "Companies", label: "Companies" }
]

const tableFields = {
  "(SELECT j.JobId, j.Title AS JobTitle, c.CompanyName AS CompanyName, j.JobType AS JobType, j.SalaryRange AS SalaryRange, j.PostedDate AS PostedDate, j.CompanyId AS CompanyId FROM Jobs j LEFT JOIN Companies c ON j.CompanyId = c.CompanyId) AS JobReport": [
    { id: "JobId", label: "Job ID", type: "number" },
    { id: "JobTitle", label: "Job Title", type: "string" },
    { id: "CompanyName", label: "Company Name", type: "string" },
    { id: "JobType", label: "Job Type", type: "string" },
    { id: "SalaryRange", label: "Salary Range", type: "string" },
    { id: "PostedDate", label: "Posted Date", type: "date" },
    { id: "CompanyId", label: "Company ID", type: "number" }
  ],
  "(SELECT a.ApplicationId, u.FullName AS UserName, u.Email AS UserEmail, j.Title AS JobTitle, c.CompanyName AS CompanyName, a.AppliedDate AS AppliedDate, a.ApplicationStatus AS Status, j.CompanyId AS CompanyId FROM Applications a JOIN Users u ON a.UserId = u.UserId JOIN Jobs j ON a.JobId = j.JobId LEFT JOIN Companies c ON j.CompanyId = c.CompanyId) AS AppReport": [
    { id: "ApplicationId", label: "App ID", type: "number" },
    { id: "UserName", label: "User Name", type: "string" },
    { id: "UserEmail", label: "User Email", type: "string" },
    { id: "JobTitle", label: "Job Title", type: "string" },
    { id: "CompanyName", label: "Company Name", type: "string" },
    { id: "AppliedDate", label: "Applied Date", type: "date" },
    { id: "Status", label: "Status", type: "string" },
    { id: "CompanyId", label: "Company ID", type: "number" }
  ],
  Users: [
    { id: "UserId", label: "User ID", type: "number" },
    { id: "FullName", label: "Full Name", type: "string" },
    { id: "Email", label: "Email", type: "string" },
    { id: "Role", label: "Role", type: "string" },
    { id: "CreatedAt", label: "Created At", type: "date" }
  ],
  Companies: [
    { id: "CompanyId", label: "Company ID", type: "number" },
    { id: "CompanyName", label: "Company Name", type: "string" },
    { id: "Industry", label: "Industry", type: "string" },
    { id: "Location", label: "Location", type: "string" }
  ]
}

const currentFields = ref([])

onMounted(async () => {
  await loadReports()
})

const loadReports = async () => {
  try {
    reports.value = await getReportConfigs()
  } catch (err) {
    handleError(err, "Load Error")
  }
}

const onTableChange = () => {
  currentFields.value = tableFields[form.value.baseTable] || []
  selectedFields.value = []
}

watch(selectedFields, (newVal) => {
  form.value.selectedFields = JSON.stringify(newVal)
})

const saveConfiguration = async () => {
  if (selectedFields.value.length === 0) {
    handleError("Please select at least one field.", "Validation Error")
    return
  }

  try {
    loading.value = true
    if (editingReport.value) {
      await updateReportConfig(editingReport.value.reportId, form.value)
      handleSuccess("Configuration updated successfully!")
    } else {
      await createReportConfig(form.value)
      handleSuccess("Configuration saved successfully!")
    }
    resetForm()
    await loadReports()
  } catch (err) {
    handleError(err, "Save Error")
  } finally {
    loading.value = false
  }
}

const editReport = (report) => {
  editingReport.value = report
  form.value = { ...report }
  selectedFields.value = JSON.parse(report.selectedFields)
  currentFields.value = tableFields[report.baseTable] || []
}

const confirmDelete = async (report) => {
  if (confirm(`Are you sure you want to delete the report "${report.reportName}"?`)) {
    try {
      loading.value = true
      await deleteReportConfig(report.reportId)
      handleSuccess("Report deleted successfully!")
      await loadReports()
    } catch (err) {
      handleError(err, "Delete Error")
    } finally {
      loading.value = false
    }
  }
}

const getSourceLabel = (baseTable) => {
  const table = availableTables.find(t => t.id === baseTable)
  return table ? table.label : baseTable
}

const resetForm = () => {
  editingReport.value = null
  form.value = {
    reportName: "",
    description: "",
    baseTable: "",
    selectedFields: "[]",
    filters: "[]"
  }
  selectedFields.value = []
}

const viewReport = (report) => {
  router.push({ path: "/central/report-viewer", query: { id: report.reportId } })
}

const parseFields = (fieldsJson) => {
  try {
    return JSON.parse(fieldsJson)
  } catch {
    return []
  }
}
</script>

<style scoped>
.report-management-page {
  padding: 30px;
  background: #f8fafc;
  min-height: 100vh;
}

.page-header h2 {
  color: #1e293b;
}

.card {
  border-radius: 12px;
}

.card-header {
  border-bottom: 1px solid #f1f5f9;
}

.fields-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 10px;
  max-height: 200px;
  overflow-y: auto;
}

.form-check-label {
  font-size: 0.9rem;
  color: #475569;
}

.table th {
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  color: #64748b;
}

.badge {
  font-weight: 500;
  padding: 6px 10px;
}

.btn-sm {
  padding: 6px 8px;
}
</style>
