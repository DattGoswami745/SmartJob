<template>
  <div class="setup-wrapper">
    <div class="setup-container">
      <div class="text-center mb-5">
        <div class="icon-box mx-auto mb-3 shadow-sm">
          <Building2 class="text-primary" size="32" />
        </div>
        <h2 class="fw-bold gradient-text">Company Onboarding</h2>
        <p class="text-muted">Welcome! Let's get your company workspace ready.</p>
      </div>

      <!-- Action Selection -->
      <div class="row g-4 mb-5" v-if="!action">
        <div class="col-md-6">
          <div class="option-card p-4 text-center cursor-pointer h-100 shadow-sm" @click="setAction('create')">
            <div class="option-icon bg-primary-soft mx-auto mb-3">
              <PlusCircle class="text-primary" size="28" />
            </div>
            <h5 class="fw-bold">Register Company</h5>
            <p class="text-muted small">I'm the first recruiter from my company.</p>
          </div>
        </div>
        <div class="col-md-6">
          <div class="option-card p-4 text-center cursor-pointer h-100 shadow-sm" @click="setAction('join')">
            <div class="option-icon bg-success-soft mx-auto mb-3">
              <UserPlus class="text-success" size="28" />
            </div>
            <h5 class="fw-bold">Join Existing</h5>
            <p class="text-muted small">My company is already on SmartJob.</p>
          </div>
        </div>
      </div>

      <!-- CREATE FORM -->
      <div v-if="action === 'create'" class="form-fade-in">
        <div class="d-flex align-items-center gap-2 mb-4 cursor-pointer text-primary small fw-bold" @click="action = null">
          <ArrowLeft size="16" /> Back to options
        </div>
        <h5 class="fw-bold mb-4">Register New Company</h5>
        
        <div class="mb-4">
          <label class="form-label small fw-bold text-muted">Company Name</label>
          <input v-model="form.companyName" type="text" class="form-control premium-input" placeholder="e.g. Acme Corp">
        </div>
        <div class="mb-4">
          <label class="form-label small fw-bold text-muted">Industry</label>
          <input v-model="form.industry" type="text" class="form-control premium-input" placeholder="e.g. Technology">
        </div>
        <div class="mb-4">
          <label class="form-label small fw-bold text-muted">Location</label>
          <input v-model="form.location" type="text" class="form-control premium-input" placeholder="e.g. New York, NY">
        </div>

        <button class="btn btn-primary-gradient w-100 py-3 shadow-sm mt-2" @click="handleSetup" :disabled="loading">
          {{ loading ? 'Creating...' : 'Register and Continue' }}
        </button>
      </div>

      <!-- JOIN LIST -->
      <div v-if="action === 'join'" class="form-fade-in">
        <div class="d-flex align-items-center gap-2 mb-4 cursor-pointer text-primary small fw-bold" @click="action = null">
          <ArrowLeft size="16" /> Back to options
        </div>
        <h5 class="fw-bold mb-4">Select Your Company</h5>

        <div class="company-list mb-4">
          <div v-for="c in companies" :key="c.companyId" 
               class="list-item d-flex justify-content-between align-items-center p-3 mb-2 shadow-sm"
               :class="{ active: selectedCompanyId === c.companyId }"
               @click="selectedCompanyId = c.companyId">
            <div>
              <div class="fw-bold">{{ c.companyName }}</div>
              <div class="text-muted extra-small">{{ c.industry || 'No industry specified' }}</div>
            </div>
            <CheckCircle v-if="selectedCompanyId === c.companyId" class="text-success" size="20" />
          </div>
          <div v-if="companies.length === 0" class="text-center py-4 text-muted small">
            No companies found. Try registering a new one!
          </div>
        </div>

        <button class="btn btn-primary-gradient w-100 py-3 shadow-sm" @click="handleSetup" :disabled="!selectedCompanyId || loading">
          {{ loading ? 'Joining...' : 'Join Company' }}
        </button>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="alert alert-danger mt-4 small py-2 d-flex align-items-center gap-2">
         <AlertCircle size="16" /> {{ error }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import { Building2, PlusCircle, UserPlus, ArrowLeft, CheckCircle, AlertCircle } from "lucide-vue-next"
import { getSetupCompanies, setupCompany } from "@/services/api"

const router = useRouter()
const action = ref(null)
const loading = ref(false)
const error = ref("")
const companies = ref([])
const selectedCompanyId = ref(null)

const form = ref({
  companyName: "",
  industry: "",
  location: ""
})

async function setAction(val) {
  action.value = val
  if (val === 'join') {
    loading.value = true
    try {
      companies.value = await getSetupCompanies()
    } catch (err) {
      error.value = "Failed to load companies"
    } finally {
      loading.value = false
    }
  }
}

async function handleSetup() {
  error.value = ""
  loading.value = true
  
  try {
    const payload = {
      action: action.value,
      companyName: form.value.companyName,
      industry: form.value.industry,
      location: form.value.location,
      companyId: selectedCompanyId.value
    }
    
    await setupCompany(payload)
    
    // Update local storage so router guard picks it up
    localStorage.setItem("userRole", "Company") 
    // We'll need to refresh the session or manually set some flag
    // Let's just redirect and the guard will re-verify via api
    router.push("/company/dashboard")
  } catch (err) {
    error.value = err.message || "Setup failed"
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.setup-wrapper {
  min-height: 80vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.setup-container {
  width: 100%;
  max-width: 600px;
  background: white;
  padding: 3rem;
  border-radius: 24px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.05);
}

.gradient-text {
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.icon-box {
  width: 64px;
  height: 64px;
  background: rgba(59, 130, 246, 0.05);
  border: 1px dashed rgba(59, 130, 246, 0.2);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.option-card {
  border-radius: 20px;
  border: 1px solid #f1f5f9;
  transition: all 0.3s;
  cursor: pointer;
}

.option-card:hover {
  transform: translateY(-5px);
  border-color: #3b82f6;
  background: rgba(59, 130, 246, 0.02);
}

.option-icon {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.bg-primary-soft { background: rgba(59, 130, 246, 0.1); }
.bg-success-soft { background: rgba(16, 185, 129, 0.1); }

.premium-input {
  border-radius: 12px;
  padding: 12px;
  border: 1px solid #e2e8f0;
}

.premium-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.btn-primary-gradient {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  font-weight: 600;
  border-radius: 12px;
  transition: all 0.3s;
}

.btn-primary-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.25);
}

.list-item {
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  cursor: pointer;
  transition: all 0.2s;
}

.list-item:hover {
  background: #f8fafc;
}

.list-item.active {
  border-color: #10b981;
  background: rgba(16, 185, 129, 0.02);
}

.extra-small { font-size: 0.7rem; }

.form-fade-in {
  animation: slideUp 0.4s ease-out;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
