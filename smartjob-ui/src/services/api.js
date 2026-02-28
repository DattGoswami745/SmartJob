const BASE = "https://localhost:7269/api"

/* ===================== DASHBOARD ===================== */
export async function getDashboardData() {
  const res = await fetch(`${BASE}/dashboard`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== JOBS ===================== */
export async function getJobs() {
  const res = await fetch(`${BASE}/jobs`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== APPLY JOB ===================== */
export async function applyJob(jobId) {
  const res = await fetch(`${BASE}/applications`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ jobId })
  })

  if (!res.ok) throw new Error(await res.text())
  return true
}

/* ===================== AUTH ===================== */
export async function loginUser(email, password) {
  const res = await fetch(`${BASE}/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password })
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function signupUser(data) {
  const res = await fetch(`${BASE}/auth/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data)
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function logoutUser() {
  const res = await fetch(`${BASE}/auth/logout`, {
    method: "POST",
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())

  localStorage.clear()
}

/* ===================== PROFILE ===================== */

export async function getProfile() {
  const res = await fetch(`${BASE}/profile`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function updateProfile(profile) {
  const res = await fetch(`${BASE}/profile`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({
      skills: profile.skills,
      education: profile.education,
      experienceYears: profile.experienceYears,
      preferredLocation: profile.preferredLocation,
      resumePath: profile.resumePath || ""
    })
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== MY APPLICATIONS ===================== */
export async function getMyApplications() {
  const res = await fetch(`${BASE}/applications`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== RESUME ===================== */

export async function getResumeSuggestions() {
  const res = await fetch(`${BASE}/resume/suggestions`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function downloadResumeFile(resume) {
  const res = await fetch(`${BASE}/resume/download`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({
      fullName: resume.fullName,
      email: resume.email,
      summary: resume.summary.map(x => x.text),
      skills: resume.skills.map(x => x.text),
      experience: resume.experience.map(x => x.text)
    })
  })

  if (!res.ok) throw new Error(await res.text())

  return await res.blob()
}

/* ===================== CHAT ===================== */
export async function sendGeminiMessage(message) {
  const res = await fetch(`${BASE}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ message })
  })

  const text = await res.text()

  if (!res.ok) throw new Error(text)

  return text
}

/* ===================== CENTRAL DASHBOARD ===================== */
export async function getCentralDashboardData() {
  const res = await fetch(`${BASE}/central/dashboard`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== CENTRAL USERS ===================== */
export async function getAllUsers() {
  const res = await fetch(`${BASE}/central/users`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== CENTRAL APPLICATIONS ===================== */
export async function getAllApplications() {
  const res = await fetch(`${BASE}/central/applications`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== CENTRAL JOBS ===================== */

export async function getCentralJobs() {
  const res = await fetch(`${BASE}/central/jobs`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function addJob(job) {
  const res = await fetch(`${BASE}/central/jobs/add`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(job)
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== CENTRAL COMPANIES ===================== */

export async function getCentralCompanies() {
  const res = await fetch(`${BASE}/central/companies`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

export async function addCompany(company) {
  const res = await fetch(`${BASE}/central/companies`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(company)
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.json()
}

/* ===================== EXPORT APPLICATIONS ===================== */

export async function exportApplicationsByJob(jobId) {
  const res = await fetch(`${BASE}/central/export/${jobId}`, {
    credentials: "include"
  })

  if (!res.ok) throw new Error(await res.text())
  return await res.blob()
}