<template>
  <div class="user-dashboard-wrapper">
<!-- Removed local toast in favor of centralized one -->

    <div class="dashboard-header mb-5">
      <h2 class="fw-bold m-0 gradient-text">My Profile</h2>
      <p class="text-muted mt-1">Manage your personal information and resume details.</p>
    </div>

    <div class="row justify-content-center">
      <div class="col-12 col-lg-8">
        <div class="premium-table-card position-relative overflow-hidden mb-5">
          <!-- Background Glow -->
          <div class="card-glow border-glow-blue"></div>

          <div class="card-content position-relative z-index-2">
            <h5 class="fw-bold text-main mb-4 pb-3 border-bottom border-light d-flex align-items-center gap-2">
              <UserCircle class="text-primary" size="24" /> Personal Details
            </h5>

            <div class="row g-4 mb-4">
              <!-- FULL NAME -->
              <div class="col-md-6">
                <label class="form-label text-muted small fw-semibold mb-2">Full Name</label>
                <div class="input-icon-wrapper">
                  <input class="form-control premium-input disabled-input" v-model="profile.fullName" disabled />
                </div>
              </div>

              <!-- EMAIL -->
              <div class="col-md-6">
                <label class="form-label text-muted small fw-semibold mb-2">Email Address</label>
                <div class="input-icon-wrapper">
                  <input class="form-control premium-input disabled-input" v-model="profile.email" disabled />
                </div>
              </div>
            </div>

            <h5 class="fw-bold text-main mb-4 mt-5 pb-3 border-bottom border-light d-flex align-items-center gap-2">
              <Briefcase class="text-primary" size="24" /> Professional Background
            </h5>

            <div class="row g-4 mb-4">
              <!-- EXPERIENCE -->
              <div class="col-md-6">
                <label class="form-label text-muted small fw-semibold mb-2">Experience (Years)</label>
                <div class="input-icon-wrapper">
                  <input type="number" class="form-control premium-input" v-model="profile.experienceYears" placeholder="e.g. 5" />
                </div>
              </div>

              <!-- EDUCATION -->
              <div class="col-md-6">
                <label class="form-label text-muted small fw-semibold mb-2">Education / Degree</label>
                <div class="input-icon-wrapper">
                  <input class="form-control premium-input" v-model="profile.education" placeholder="e.g. B.S. in Computer Science" />
                </div>
              </div>

              <!-- LOCATION -->
              <div class="col-12">
                <label class="form-label text-muted small fw-semibold mb-2">Preferred Location</label>
                <div class="input-icon-wrapper">
                  <input class="form-control premium-input" v-model="profile.preferredLocation" placeholder="e.g. Remote, or New York" />
                </div>
              </div>

              <!-- RESUME UPLOAD -->
              <div class="col-12 mt-4">
                <label class="form-label text-muted small fw-semibold mb-2">Resume (PDF or Word)</label>
                <div class="d-flex align-items-center gap-3">
                  <input type="file" ref="resumeInput" class="form-control premium-input flex-grow-1" style="max-width: 400px;" accept=".pdf,.doc,.docx" @change="handleResumeUpload" />
                  <button v-if="profile.resumePath" @click="openResumeViewer" class="btn btn-outline-primary d-flex align-items-center gap-2">
                    <FileText size="18" /> View Current Resume
                  </button>
                </div>
              </div>
            </div>

            <h5 class="fw-bold text-main mb-4 mt-5 pb-3 border-bottom border-light d-flex align-items-center gap-2">
              <Award class="text-primary" size="24" /> Skills & Expertise
            </h5>

            <!-- YOUR SKILLS -->
            <div class="mb-4 p-4 rounded-3 bg-light-subtle border border-light">
              <label class="form-label text-main fw-semibold mb-3 d-block">Your Selected Skills</label>
              
              <div v-if="selectedSkills.length === 0" class="text-muted small fst-italic mb-2">
                No skills added yet. Search below to add some!
              </div>

              <div class="d-flex flex-wrap gap-2">
                <transition-group name="skill-pop" tag="div" class="d-flex flex-wrap gap-2">
                  <button
                    v-for="skill in selectedSkills"
                    :key="skill"
                    class="btn skill-badge selected d-flex align-items-center gap-1"
                    @click="removeSkill(skill)"
                    title="Click to remove"
                  >
                    {{ skill }} <X size="14" class="ms-1 opacity-75" />
                  </button>
                </transition-group>
              </div>
            </div>

            <!-- SKILL SEARCH -->
            <div class="mb-3">
              <label class="form-label text-muted small fw-semibold mb-2">Search to Add Skills</label>
              <div class="position-relative">
                <Search class="position-absolute top-50 start-0 translate-middle-y ms-3 text-muted opacity-50" size="18" />
                <input
                  class="form-control premium-input ps-5"
                  v-model="skillSearch"
                  placeholder="Type skill name e.g. 'Vue'..."
                />
              </div>
            </div>

            <!-- SUGGESTED SKILLS -->
            <div class="mb-5">
              <transition-group name="skill-pop" tag="div" class="d-flex flex-wrap gap-2 mt-3">
                <button
                  v-for="skill in filteredSkills"
                  :key="skill"
                  class="btn skill-badge custom-outline d-flex align-items-center gap-1"
                  @click="addSkill(skill)"
                >
                  <Plus size="14" class="text-primary opacity-75" /> {{ skill }}
                </button>
              </transition-group>
              <div v-if="skillSearch && filteredSkills.length === 0" class="text-muted small mt-2">
                No matching skills found.
              </div>
            </div>

            <div class="border-top border-light pt-4 mt-2 d-flex justify-content-end">
              <button class="btn btn-primary-gradient px-5 py-2 fw-bold d-flex align-items-center gap-2" @click="handleUpdateProfile">
                <Save size="18" /> Save Changes
              </button>
            </div>

          </div>
        </div>
      </div>
    </div>

    <!-- 📄 RESUME MODAL -->
    <ResumeModal 
      :isOpen="isResumeModalOpen" 
      :resumeUrl="resumeViewerUrl" 
      @close="isResumeModalOpen = false" 
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { CheckCircle2, AlertCircle, UserCircle, Briefcase, Award, Search, Plus, X, Save, FileText } from "lucide-vue-next"
import { getProfile, updateProfile, API_HOST } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"
import ResumeModal from "@/components/ResumeModal.vue"

/* PROFILE */
const profile = ref({
  fullName: "",
  email: "",
  skills: "",
  experienceYears: 0,
  education: "",
  preferredLocation: "",
  resumePath: ""
})

const resumeInput = ref(null)

/* 📄 RESUME VIEWER STATE */
const isResumeModalOpen = ref(false)
const resumeViewerUrl = ref("")

const openResumeViewer = () => {
  if (profile.value.resumePath) {
    resumeViewerUrl.value = `${API_HOST}${profile.value.resumePath}`
    isResumeModalOpen.value = true
  }
}

/* SKILLS */
const skillSearch = ref("")
const selectedSkills = ref([])

const allSkills = [
  "Java", "C#", "Python", "JavaScript", "Vue.js", "React",
  "ASP.NET", "SQL", "MongoDB", "Firebase",
  "HTML", "CSS", "Node.js", "Docker", "AWS",
  "TailwindCSS", "TypeScript", "Angular", "Redis", "Git"
]

const filteredSkills = computed(() => {
  return allSkills.filter(s =>
    s.toLowerCase().includes(skillSearch.value.toLowerCase()) &&
    !selectedSkills.value.includes(s)
  )
})

const addSkill = (skill) => {
  selectedSkills.value.push(skill)
  skillSearch.value = ""
}

const removeSkill = (skill) => {
  selectedSkills.value = selectedSkills.value.filter(s => s !== skill)
}

onMounted(async () => {
  try {
    const data = await getProfile()
    profile.value = data

    if (data.skills) {
      selectedSkills.value = data.skills.split(",").map(s => s.trim()).filter(s => s)
    }
  } catch (err) {
    handleError(err, "Load Error")
  }
})

const handleUpdateProfile = async () => {
  try {
    // Sync skills before update
    profile.value.skills = selectedSkills.value.join(",")
    await updateProfile(profile.value)
    handleSuccess("Profile updated successfully!")
  } catch (err) {
    handleError(err, "Update Error")
  }
}

const handleResumeUpload = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append("file", file)

  try {
    const res = await fetch(`${API_HOST}/api/profile/upload-resume`, {
      method: "POST",
      credentials: "include",
      body: formData
    })

    if (res.ok) {
      const data = await res.json()
      profile.value.resumePath = data.resumePath
      handleSuccess("Resume uploaded successfully!")
    } else {
      const errData = await res.json()
      handleError(errData.message || "Failed to upload resume", "Upload Error")
      if (resumeInput.value) resumeInput.value.value = ""
    }
  } catch (err) {
    handleError(err, "Upload Error")
    if (resumeInput.value) resumeInput.value.value = ""
  }
}
</script>

<style scoped>
.user-dashboard-wrapper {
  padding: 0;
}

/* Gradient Text for Header */
.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* Premium Layout Container */
.premium-table-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 40px;
  box-shadow: 0 15px 35px -15px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border);
  position: relative;
  z-index: 1;
}

/* Glow Effects */
.card-glow {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 4px;
  opacity: 0.15;
}
.border-glow-blue { background: linear-gradient(90deg, #3b82f6, #8b5cf6); }

/* Form Elements */
.premium-input {
  background: var(--bg-main) !important;
  color: var(--text-main) !important;
  border: 1px solid var(--border) !important;
  border-radius: 12px !important;
  padding: 12px 16px !important;
  transition: all 0.3s ease !important;
}

.premium-input:focus {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.15) !important;
  outline: none !important;
}

.premium-input::placeholder {
  color: var(--text-muted);
  opacity: 0.5;
}

.disabled-input {
  background: var(--recent-bg) !important;
  opacity: 0.7;
  cursor: not-allowed;
}

/* Skill Badges */
.skill-badge {
  border-radius: 10px;
  padding: 8px 16px;
  font-weight: 500;
  font-size: 0.875rem;
  transition: all 0.2s ease;
  letter-spacing: 0.3px;
}

.skill-badge.selected {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.3);
}

.skill-badge.selected:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(59, 130, 246, 0.4);
  background: linear-gradient(135deg, #ef4444, #dc2626); /* turn red on hover to indicate remove */
}

.skill-badge.custom-outline {
  background: var(--bg-main);
  color: var(--text-main);
  border: 1px dashed var(--border);
}

.skill-badge.custom-outline:hover {
  border-color: #3b82f6;
  border-style: solid;
  color: #3b82f6;
  background: rgba(59, 130, 246, 0.05);
  transform: translateY(-2px);
}

/* Save Button */
.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(59, 130, 246, 0.3);
  transition: all 0.3s ease;
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
  color: white;
}

/* Theme Utilities */
.text-main { color: var(--text-main); }
.border-light { border-color: var(--border) !important; }
.bg-light-subtle { background-color: var(--recent-bg) !important; }

/* Transitions */
.skill-pop-enter-active,
.skill-pop-leave-active {
  transition: all 0.3s ease;
}
.skill-pop-enter-from,
.skill-pop-leave-to {
  opacity: 0;
  transform: scale(0.9);
}
.skill-pop-move {
  transition: transform 0.3s ease;
}
</style>
