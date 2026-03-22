const BASE = "https://localhost:7269/api"
export const API_HOST = "https://localhost:7269"

/**
 * Common helper to handle fetch responses and standardize errors.
 * @param {Response} res 
 * @returns {Promise<any>}
 */
async function handleResponse(res) {
  if (res.ok) {
    const contentType = res.headers.get("content-type");
    
    if (contentType && contentType.includes("application/json")) {
      const text = await res.text();
      return text ? JSON.parse(text) : true;
    }

    // Text responses (like from the Chat endpoint)
    if (contentType && contentType.includes("text/")) {
      return await res.text();
    }

    // Binary responses (Blob)
    if (contentType && !contentType.includes("application/json")) {
      return await res.blob();
    }

    // Fallback for missing content type
    const text = await res.text();
    return text ? JSON.parse(text) : true;
  }

  // Handle errors
  let errorMessage = "An error occurred";
  try {
    const errorData = await res.json();
    errorMessage = errorData.message || JSON.stringify(errorData);
  } catch (e) {
    const textError = await res.text();
    errorMessage = textError || `HTTP Error ${res.status}`;
  }
  
  throw new Error(errorMessage);
}

/* ===================== DASHBOARD ===================== */
export async function getDashboardData() {
  const res = await fetch(`${BASE}/dashboard`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== JOBS ===================== */
export async function getJobs() {
  const res = await fetch(`${BASE}/jobs`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== APPLY JOB ===================== */
export async function applyJob(jobId) {
  const res = await fetch(`${BASE}/applications`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ jobId })
  })
  return handleResponse(res)
}

/* ===================== AUTH ===================== */
export async function loginUser(email, password) {
  const res = await fetch(`${BASE}/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password })
  })
  return handleResponse(res)
}

export async function signupUser(data) {
  const res = await fetch(`${BASE}/auth/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data)
  })
  return handleResponse(res)
}

export async function logoutUser() {
  const res = await fetch(`${BASE}/auth/logout`, {
    method: "POST",
    credentials: "include"
  })
  await handleResponse(res)
  localStorage.clear()
}

export async function verifyEmail(email, otp) {
  const res = await fetch(`${BASE}/auth/verify-email`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, otp })
  })
  return handleResponse(res)
}

export async function resendOTP(email) {
  const res = await fetch(`${BASE}/auth/resend-otp`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email })
  })
  return handleResponse(res)
}


/* ===================== PROFILE ===================== */

export async function getProfile() {
  const res = await fetch(`${BASE}/profile`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function updateProfile(profile) {
  const res = await fetch(`${BASE}/profile`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({
      Skills: Array.isArray(profile.skills) ? profile.skills : (profile.skills ? profile.skills.split(",").map(s => s.trim()).filter(x => x) : []),
      Education: profile.education,
      ExperienceYears: profile.experienceYears,
      PreferredLocation: profile.preferredLocation,
      ResumePath: profile.resumePath || ""
    })
  })
  return handleResponse(res)
}

/* ===================== MY APPLICATIONS ===================== */
export async function getMyApplications() {
  const res = await fetch(`${BASE}/applications`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== RESUME ===================== */

export async function getResumeSuggestions(sections) {
  const res = await fetch(`${BASE}/resume/suggestions`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(sections)
  })
  return handleResponse(res)
}

export async function downloadResumeFile(resume) {
  const res = await fetch(`${BASE}/resume/download`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({
      fullName: resume.fullName,
      email: resume.email,
      sections: resume.sections.map(sec => ({
        title: sec.title,
        items: sec.items.map(x => x.text)
      }))
    })
  })
  return handleResponse(res)
}

export async function downloadResumePdf(resume) {
  const res = await fetch(`${BASE}/resume/download-pdf`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({
      fullName: resume.fullName,
      email: resume.email,
      sections: resume.sections.map(sec => ({
        title: sec.title,
        items: sec.items.map(x => x.text)
      }))
    })
  })
  return handleResponse(res)
}

export async function downloadResumeFileHtml(htmlContent) {
  const res = await fetch(`${BASE}/resume/download-html`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ html: htmlContent })
  })
  return handleResponse(res)
}

export async function downloadResumePdfHtml(htmlContent) {
  const res = await fetch(`${BASE}/resume/download-pdf-html`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ html: htmlContent })
  })
  return handleResponse(res)
}

/* ===================== CHAT ===================== */
export async function sendGeminiMessage(message) {
  const res = await fetch(`${BASE}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ message })
  })
  
  // Chat returns plain text, handleResponse handles it
  return handleResponse(res)
}

/* ===================== CENTRAL DASHBOARD ===================== */
export async function getCentralDashboardData() {
  const res = await fetch(`${BASE}/central/dashboard`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== COMPANY DASHBOARD ===================== */
export async function getCompanyDashboardData() {
  const res = await fetch(`${BASE}/company/dashboard`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function getSetupCompanies() {
  const res = await fetch(`${BASE}/company/setup/list`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function setupCompany(data) {
  const res = await fetch(`${BASE}/company/setup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(data)
  })
  return handleResponse(res)
}

/* ===================== COMPANY JOBS ===================== */

export async function getCompanyJobs() {
  const res = await fetch(`${BASE}/company/jobs`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function addCompanyJob(formData) {
  const res = await fetch(`${BASE}/company/jobs/add`, {
    method: "POST",
    credentials: "include",
    body: formData
  })
  return handleResponse(res)
}

export async function updateCompanyJob(jobId, formData) {
  const res = await fetch(`${BASE}/company/jobs/update/${jobId}`, {
    method: "PUT",
    credentials: "include",
    body: formData
  })
  return handleResponse(res)
}

/* ===================== COMPANY APPLICATIONS ===================== */

export async function getCompanyApplications() {
  const res = await fetch(`${BASE}/company/applications`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function deleteCompanyApplication(appId) {
  const res = await fetch(`${BASE}/company/applications/${appId}`, {
    method: "DELETE",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function getUserProfileForCompany(userId) {
  const res = await fetch(`${BASE}/company/applications/profile/${userId}`, {
    credentials: "include"
  })
  
  if (res.status === 404) return null
  return handleResponse(res)
}

export async function downloadCompanyApplicationsReport(search = "", jobId = "") {
  const query = new URLSearchParams({ search, jobId }).toString()
  window.open(`${BASE}/company/applications/report?${query}`, "_blank")
}

export async function markCandidateAsPlaced(appId) {
  const res = await fetch(`${BASE}/company/applications/mark-placed/${appId}`, {
    method: "POST",
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== CENTRAL USERS ===================== */
export async function getAllUsers() {
  const res = await fetch(`${BASE}/central/users`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== CENTRAL APPLICATIONS ===================== */
export async function getAllApplications() {
  const res = await fetch(`${BASE}/central/applications`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function deleteCentralApplication(appId) {
  const res = await fetch(`${BASE}/central/applications/${appId}`, {
    method: "DELETE",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function getUserProfileForAdmin(userId) {
  const res = await fetch(`${BASE}/central/applications/profile/${userId}`, {
    credentials: "include"
  })
  
  if (res.status === 404) return null
  return handleResponse(res)
}

/* ===================== CENTRAL USERS ===================== */

export async function getCentralUsers() {
  const res = await fetch(`${BASE}/central/users`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function deleteCentralUser(userId) {
  const res = await fetch(`${BASE}/central/users/${userId}`, {
    method: "DELETE",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function getUserActivityLogs(userId) {
  const res = await fetch(`${BASE}/central/users/${userId}/activity`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== CENTRAL REPORTS ===================== */

export async function downloadJobReport(jobId) {
  const res = await fetch(`${BASE}/central/reports/job/${jobId}`)

  if (!res.ok) {
    if (res.status === 404) throw new Error("No applicants found for this job.")
    throw new Error(await res.text())
  }

  const blob = await res.blob()
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url
  a.download = `JobReport_${jobId}.xls`
  document.body.appendChild(a)
  a.click()
  a.remove()
  window.URL.revokeObjectURL(url)
}

export async function downloadCentralMultiReport(companyId, jobId) {
  const query = new URLSearchParams()
  if (companyId) query.append("companyId", companyId)
  if (jobId) query.append("jobId", jobId)

  const res = await fetch(`${BASE}/central/reports/multi?${query.toString()}`, {
    credentials: "include"
  })

  if (!res.ok) {
    if (res.status === 404) throw new Error("No applicants found for the selected filters.")
    throw new Error(await res.text())
  }

  const blob = await res.blob()
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url
  a.download = `Consolidated_Report.xls`
  document.body.appendChild(a)
  a.click()
  a.remove()
  window.URL.revokeObjectURL(url)
}

export async function getCompanies() {
  const res = await fetch(`${BASE}/central/companies`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== CENTRAL JOBS ===================== */

export async function getCentralJobs(status = "All") {
  const res = await fetch(`${BASE}/central/jobs?status=${status}`, {
    method: "GET",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function addJob(job) {
  const res = await fetch(`${BASE}/central/jobs/add`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(job)
  })
  return handleResponse(res)
}

export async function updateJob(jobId, job) {
  const res = await fetch(`${BASE}/central/jobs/update/${jobId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(job)
  })
  return handleResponse(res)
}

export async function approveJob(jobId) {
  const res = await fetch(`${BASE}/central/jobs/approve/${jobId}`, {
    method: "POST",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function rejectJob(jobId) {
  const res = await fetch(`${BASE}/central/jobs/reject/${jobId}`, {
    method: "POST",
    credentials: "include"
  })
  return handleResponse(res)
}

export async function restoreJob(jobId) {
  const res = await fetch(`${BASE}/central/jobs/restore/${jobId}`, {
    method: "POST",
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== CENTRAL COMPANIES ===================== */

export async function getCentralCompanies() {
  const res = await fetch(`${BASE}/central/companies`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function addCompany(company) {
  const res = await fetch(`${BASE}/central/companies`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(company)
  })
  return handleResponse(res)
}

/* ===================== EXPORT APPLICATIONS ===================== */

export async function exportApplicationsByJob(jobId) {
  const res = await fetch(`${BASE}/central/export/${jobId}`, {
    credentials: "include"
  })
  return handleResponse(res)
}

/* ===================== COMPANY VERIFICATION ===================== */

export async function uploadVerificationDocuments(formData) {
  const res = await fetch(`${BASE}/company/upload-verification-documents`, {
    method: "POST",
    credentials: "include",
    body: formData
  })
  return handleResponse(res)
}

export async function getCompanyVerificationDocuments(companyId) {
  const res = await fetch(`${BASE}/admin/company-documents/${companyId}`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function checkSession() {
  const res = await fetch(`${BASE}/auth/check-session`, {
    credentials: "include"
  })
  return handleResponse(res)
}

export async function verifyCompany(dto) {
  const res = await fetch(`${BASE}/admin/verify-company`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(dto)
  })
  return handleResponse(res)
}
/* ===================== DYNAMIC REPORTS ===================== */

export async function getReportConfigs() {
  const res = await fetch(`${BASE}/reports/config`, { credentials: "include" })
  return handleResponse(res)
}

export async function getReportConfig(id) {
  const res = await fetch(`${BASE}/reports/config/${id}`, { credentials: "include" })
  return handleResponse(res)
}

export async function createReportConfig(config) {
  const res = await fetch(`${BASE}/reports/config`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(config)
  })
  return handleResponse(res)
}

export async function updateReportConfig(id, config) {
  const res = await fetch(`${BASE}/reports/config/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(config)
  })
  return handleResponse(res)
}

export async function generateReportData(id, request) {
  const res = await fetch(`${BASE}/reports/generate/${id}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(request)
  })
  return handleResponse(res)
}

export async function downloadDynamicReport(id, format, filters) {
  const filterJson = encodeURIComponent(JSON.stringify(filters))
  const res = await fetch(`${BASE}/reports/export/${id}/${format}?filterJson=${filterJson}`, {
    credentials: "include"
  })
  
  const blob = await handleResponse(res)
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url
  a.download = `Report_${id}.${format === 'excel' ? 'xlsx' : 'pdf'}`
  document.body.appendChild(a)
  a.click()
  a.remove()
  window.URL.revokeObjectURL(url)
}
/* ===================== SECURITY TOOL ===================== */

export async function encryptText(text) {
  const res = await fetch(`${BASE}/security/encrypt`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text })
  })
  return handleResponse(res)
}

export async function decryptText(text) {
  const res = await fetch(`${BASE}/security/decrypt`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text })
  })
  return handleResponse(res)
}
