<template>
  <div>
    <!-- 🔔 TOP CENTER POPUP -->
    <div
      v-if="successMessage || errorMessage"
      class="toast-msg"
      :class="successMessage ? 'success' : 'error'"
    >
      {{ successMessage || errorMessage }}
    </div>

    <h2 class="mb-4">My Profile</h2>

    <div class="card shadow-sm hover-card" style="max-width:700px">
      <div class="card-body">

        <!-- FULL NAME -->
        <div class="mb-3">
          <label class="form-label">Full Name</label>
          <input class="form-control" v-model="profile.fullName" disabled />
        </div>

        <!-- EMAIL -->
        <div class="mb-3">
          <label class="form-label">Email</label>
          <input class="form-control" v-model="profile.email" disabled />
        </div>

        <!-- SKILL SEARCH -->
        <div class="mb-2">
          <label class="form-label">Search Skills</label>
          <input
            class="form-control"
            v-model="skillSearch"
            placeholder="Type skill name..."
          />
        </div>

        <!-- SUGGESTED SKILLS -->
        <div class="mb-3">
          <button
            v-for="skill in filteredSkills"
            :key="skill"
            class="btn btn-outline-success btn-sm me-2 mb-2"
            @click="addSkill(skill)"
          >
            + {{ skill }}
          </button>
        </div>

        <!-- SELECTED SKILLS -->
        <div class="mb-3">
          <label class="form-label">Your Skills</label>
          <div>
            <button
              v-for="skill in selectedSkills"
              :key="skill"
              class="btn btn-success btn-sm me-2 mb-2"
              @click="removeSkill(skill)"
            >
              {{ skill }} ✕
            </button>
          </div>
        </div>

        <!-- EXPERIENCE -->
        <div class="mb-3">
          <label class="form-label">Experience (Years)</label>
          <input type="number" class="form-control" v-model="profile.experienceYears" />
        </div>

        <!-- EDUCATION -->
        <div class="mb-3">
          <label class="form-label">Education</label>
          <input class="form-control" v-model="profile.education" />
        </div>

        <!-- LOCATION -->
        <div class="mb-3">
          <label class="form-label">Preferred Location</label>
          <input class="form-control" v-model="profile.preferredLocation" />
        </div>

        <button class="btn btn-success" @click="updateProfile">
          Save Profile
        </button>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"

/* PROFILE */
const profile = ref({
  fullName: "",
  email: "",
  skills: "",
  experienceYears: 0,
  education: "",
  preferredLocation: ""
})

/* 🔔 POPUP STATE */
const successMessage = ref("")
const errorMessage = ref("")

/* SKILLS */
const skillSearch = ref("")
const selectedSkills = ref([])

const allSkills = [
  "Java", "C#", "Python", "JavaScript", "Vue.js", "React",
  "ASP.NET", "SQL", "MongoDB", "Firebase",
  "HTML", "CSS", "Node.js", "Docker", "AWS"
]

const filteredSkills = computed(() => {
  return allSkills.filter(s =>
    s.toLowerCase().includes(skillSearch.value.toLowerCase()) &&
    !selectedSkills.value.includes(s)
  )
})

function addSkill(skill) {
  selectedSkills.value.push(skill)
  skillSearch.value = ""
}

function removeSkill(skill) {
  selectedSkills.value = selectedSkills.value.filter(s => s !== skill)
}

onMounted(async () => {
  const res = await fetch("https://localhost:7269/api/profile", {
    credentials: "include"
  })

  const data = await res.json()
  profile.value = data

  if (data.skills) {
    selectedSkills.value = data.skills.split(",").map(s => s.trim())
  }
})

async function updateProfile() {
  successMessage.value = ""
  errorMessage.value = ""

  profile.value.skills = selectedSkills.value.join(",")

  const res = await fetch("https://localhost:7269/api/profile", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(profile.value)
  })

  if (res.ok) {
    successMessage.value = "Profile updated successfully ✅"
    setTimeout(() => successMessage.value = "", 3000)
  } else {
    errorMessage.value = "Failed to update profile ❌"
    setTimeout(() => errorMessage.value = "", 3000)
  }
}
</script>

<style scoped>
.hover-card {
  transition: 0.3s ease;
}
.hover-card:hover {
  transform: translateY(-4px);
}

/* 🔔 TOP CENTER TOAST */
.toast-msg {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  padding: 12px 22px;
  border-radius: 6px;
  font-weight: 500;
  z-index: 9999;
  animation: slideDown 0.4s ease;
}

.toast-msg.success {
  background: #198754;
  color: #fff;
}

.toast-msg.error {
  background: #dc3545;
  color: #fff;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translate(-50%, -20px);
  }
  to {
    opacity: 1;
    transform: translate(-50%, 0);
  }
}
</style>
