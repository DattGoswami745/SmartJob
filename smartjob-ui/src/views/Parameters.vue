<template>
  <div class="parameters-page d-flex align-items-center justify-content-center min-vh-100 p-4">
    <div class="container" style="max-width: 1000px;">
      <!-- Header -->
      <div class="text-center mb-5 animate-in">
        <div class="logo-box mb-3 mx-auto">
          <i class="bi bi-gear-wide-connected text-primary display-4"></i>
        </div>
        <h1 class="fw-bold tracking-tight text-main">System Parameters</h1>
        <p class="text-muted lead">Manage global configuration values and system keys</p>
      </div>

      <div class="card shadow-lg border-0 hover-lift animate-in" style="animation-delay: 0.1s">
        <div class="card-header bg-primary text-white py-4 px-4 d-flex justify-content-between align-items-center">
          <h4 class="mb-0 fw-bold"><i class="bi bi-list-columns-reverse me-2"></i> Configuration Registry</h4>
          <div class="search-wrapper position-relative" style="width: 300px;">
            <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-white-50"></i>
            <input 
              v-model="searchQuery" 
              class="form-control search-input ps-5" 
              placeholder="Search keys or descriptions..."
            />
          </div>
        </div>
        <div class="card-body p-0">
          <div v-if="loading" class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted">Fetching parameters...</p>
          </div>

          <div v-else-if="filteredParameters.length === 0" class="text-center py-5">
            <i class="bi bi-inbox text-muted display-1 opacity-25"></i>
            <p class="mt-3 text-muted">No parameters found matching your search.</p>
          </div>

          <div v-else class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="bg-light">
                <tr>
                  <th class="ps-4 py-3 text-uppercase small fw-bold text-muted">Key</th>
                  <th class="py-3 text-uppercase small fw-bold text-muted">Value</th>
                  <th class="py-3 text-uppercase small fw-bold text-muted">Description</th>
                  <th class="pe-4 py-3 text-end text-uppercase small fw-bold text-muted">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="param in filteredParameters" :key="param.paramKey" class="param-row">
                  <td class="ps-4">
                    <span class="badge bg-soft-primary text-primary px-2 py-1">{{ param.paramKey }}</span>
                  </td>
                  <td class="text-truncate" style="max-width: 250px;">
                    <code class="text-dark small">{{ param.paramValue }}</code>
                  </td>
                  <td class="text-muted small">
                    {{ param.description || 'No description provided' }}
                  </td>
                  <td class="pe-4 text-end">
                    <button class="btn btn-sm btn-outline-primary rounded-pill px-3" @click="openEditModal(param)">
                      <i class="bi bi-pencil-square me-1"></i> Edit
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Footer back link -->
      <div class="text-center mt-5 animate-in" style="animation-delay: 0.2s">
        <router-link to="/login" class="text-decoration-none text-muted">
          <i class="bi bi-arrow-left me-1"></i> Return to Login
        </router-link>
      </div>
    </div>

    <!-- EDIT MODAL -->
    <div v-if="editingParam" class="modal-overlay d-flex align-items-center justify-content-center p-3" @click.self="editingParam = null">
      <div class="modal-content-card animate-pop shadow-2xl">
        <div class="card border-0">
          <div class="card-header bg-dark text-white py-3 d-flex justify-content-between align-items-center">
            <h5 class="mb-0 fw-bold">Edit Parameter</h5>
            <button class="btn btn-sm text-white-50" @click="editingParam = null"><i class="bi bi-x-lg"></i></button>
          </div>
          <div class="card-body p-4">
            <div class="mb-4">
              <label class="form-label text-muted small fw-bold text-uppercase">Parameter Key</label>
              <input class="form-control bg-light" :value="editingParam.paramKey" disabled />
            </div>

            <div class="mb-4">
              <label class="form-label text-muted small fw-bold text-uppercase">Parameter Value</label>
              <textarea 
                v-model="editingParam.paramValue" 
                class="form-control custom-textarea" 
                rows="5"
                placeholder="Enter value here..."
              ></textarea>
            </div>

            <div class="mb-4">
              <label class="form-label text-muted small fw-bold text-uppercase">Description</label>
              <input 
                v-model="editingParam.description" 
                class="form-control" 
                placeholder="What does this parameter do?"
              />
            </div>

            <div class="d-grid">
              <button 
                class="btn btn-primary py-2 fw-bold" 
                @click="onSave" 
                :disabled="saving"
              >
                <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="bi bi-cloud-check-fill me-2"></i> Save Changes
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue"
import { getParameters, updateParameter } from "@/services/api"
import { handleError, handleSuccess } from "@/utils/error-handler"

const parameters = ref([])
const loading = ref(false)
const searchQuery = ref("")
const editingParam = ref(null)
const saving = ref(false)

const loadParameters = async () => {
  try {
    loading.value = true
    parameters.value = await getParameters()
  } catch (err) {
    handleError(err, "Load Failed")
  } finally {
    loading.value = false
  }
}

const filteredParameters = computed(() => {
  const q = searchQuery.value.toLowerCase()
  if (!q) return parameters.value
  return parameters.value.filter(p => 
    p.paramKey.toLowerCase().includes(q) || 
    (p.description && p.description.toLowerCase().includes(q))
  )
})

const openEditModal = (param) => {
  editingParam.value = { ...param }
}

const onSave = async () => {
  try {
    saving.value = true
    await updateParameter(editingParam.value.paramKey, {
      paramValue: editingParam.value.paramValue,
      description: editingParam.value.description
    })
    handleSuccess("Parameter updated successfully!")
    editingParam.value = null
    await loadParameters()
  } catch (err) {
    handleError(err, "Update Failed")
  } finally {
    saving.value = false
  }
}

onMounted(loadParameters)
</script>

<style scoped>
.parameters-page {
  background: radial-gradient(circle at top right, #f8fafc, #e2e8f0);
  background-attachment: fixed;
}

.text-main { color: #0f172a; }

.logo-box {
  width: 80px;
  height: 80px;
  background: #fff;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 25px rgba(0,0,0,0.05);
}

.card {
  border-radius: 1.25rem;
  overflow: hidden;
  transition: all 0.3s ease;
}

.hover-lift:hover {
  transform: translateY(-5px);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.1) !important;
}

/* Search Input */
.search-input {
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  border-radius: 50px;
  font-size: 0.85rem;
}

.search-input::placeholder {
  color: rgba(255, 255, 255, 0.6);
}

.search-input:focus {
  background: rgba(255, 255, 255, 0.25);
  border-color: rgba(255, 255, 255, 0.4);
  box-shadow: none;
  color: white;
}

/* Table Styles */
.param-row {
  transition: background 0.2s;
}

.param-row:hover {
  background-color: rgba(59, 130, 246, 0.02);
}

.bg-soft-primary {
  background-color: rgba(59, 130, 246, 0.1);
}

/* Modal Styling */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(4px);
  z-index: 1000;
}

.modal-content-card {
  width: 100%;
  max-width: 500px;
  border-radius: 1.5rem;
  overflow: hidden;
}

.custom-textarea {
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 1rem;
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.9rem;
  resize: none;
}

/* Animations */
.animate-in {
  animation: fadeInDown 0.6s ease-out both;
}

.animate-pop {
  animation: scaleIn 0.3s cubic-bezier(0.34, 1.56, 0.64, 1) both;
}

@keyframes fadeInDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.9); }
  to { opacity: 1; transform: scale(1); }
}

.tracking-tight {
  letter-spacing: -0.025em;
}
</style>
