<template>
  <div class="resume-wrapper">
    <div class="container py-5">
      <div class="text-center mb-5">
        <h1 class="fw-bold gradient-text">AI Resume Builder</h1>
        <p class="text-muted">Drag & drop AI suggestions to craft your professional resume</p>
      </div>

      <div class="row g-4">

        <!-- LEFT SIDE : RESUME TEMPLATE -->
        <div class="col-lg-7">
          <div class="card resume-card shadow-lg border-0">
            <div class="card-body p-4">

              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Full Name</label>
                  <input v-model="resume.fullName" class="form-control stylish-input" disabled />
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-semibold">Email</label>
                  <input v-model="resume.email" class="form-control stylish-input" disabled />
                </div>
              </div>

              <!-- DYNAMIC SECTIONS -->
              <div v-for="(section, secIndex) in resume.sections" :key="section.id" class="section mt-4 position-relative">
                <div class="d-flex align-items-center mb-2">
                  <input v-model="section.title" class="form-control section-title-input fw-bold fs-5 me-2" placeholder="Section Title" />
                  <button class="btn btn-sm btn-outline-danger ms-auto" @click="deleteSection(secIndex)" title="Delete Section">
                    <span class="fs-6">&times;</span>
                  </button>
                </div>
                
                <draggable
                  v-model="section.items"
                  :group="{ name: 'shared_items', pull: true, put: true }"
                  item-key="id"
                  class="drop-zone">
                  <template #item="{ element, index }">
                    <div class="resume-item d-flex align-items-start gap-2">
                      <span class="bullet-point mt-2">&bull;</span>
                      <textarea v-model="element.text" class="form-control stylish-textarea flex-grow-1" placeholder="Type or drop here..."></textarea>
                      <button class="remove-btn position-static mt-1" @click="removeItem(secIndex, index)">&times;</button>
                    </div>
                  </template>
                  <template #footer>
                     <div v-if="section.items.length === 0" class="text-center text-muted small py-2 fst-italic">
                       Drop items here or click Add Bullet
                     </div>
                  </template>
                </draggable>
                
                <div class="text-end mt-2">
                   <button class="btn btn-sm btn-link text-decoration-none" @click="addBullet(secIndex)">+ Add Bullet</button>
                </div>
              </div>

              <button class="btn btn-outline-primary dashed-btn w-100 mt-4 py-2" @click="addSection">
                + Add New Section
              </button>

              <button class="btn download-btn w-100 mt-4" @click="handleDownload">
                Download Resume (Word)
              </button>

            </div>
          </div>
        </div>

        <div class="col-lg-5">
          <div class="card suggestion-card shadow-lg border-0 suggestion-sticky">
            <div class="card-body p-4">
              <div class="d-flex align-items-center justify-content-between mb-3">
                <h5 class="fw-bold m-0">AI Suggestions</h5>
                <button class="btn btn-sm btn-primary rounded-pill px-3" @click="refreshSuggestions" :disabled="loading">
                  <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
                  Refresh
                </button>
              </div>

              <div v-if="loading" class="text-center py-5">
                <div class="spinner-border text-primary mb-3"></div>
                <p class="text-muted small">Generating tailored suggestions...</p>
              </div>

              <div v-else>
                <div v-if="Object.keys(availableItems).length === 0" class="text-center py-4 text-muted small">
                  No suggestions available. Try adding sections or clicking Refresh.
                </div>
                
                <div v-for="(items, category) in availableItems" :key="category" class="suggestion-section mb-4">
                  <h6 class="text-uppercase small fw-bold text-primary border-bottom pb-1 mb-2">{{ category }}</h6>
                  <draggable
                    v-model="availableItems[category]"
                    :group="{ name: 'shared_items', pull:'clone', put:false }"
                    :clone="cloneItem"
                    :sort="false"
                    item-key="id">
                    <template #item="{ element }">
                      <div class="suggestion-item border shadow-sm">{{ element.text }}</div>
                    </template>
                  </draggable>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from "vue"
import draggable from "vuedraggable"
import { getResumeSuggestions, downloadResumeFile } from "../services/api"

const resume = ref({ 
  fullName: "", 
  email: "", 
  sections: [
    { id: "sec_1", title: "Professional Summary", items: [] },
    { id: "sec_2", title: "Skills", items: [] },
    { id: "sec_3", title: "Experience", items: [] }
  ] 
})
const availableItems = ref({})
const loading = ref(true)

function cloneItem(item){
  return { id: "item_" + Date.now() + Math.random(), text: item.text }
}

async function refreshSuggestions() {
  loading.value = true
  try {
    const sectionsToRequest = resume.value.sections.map(s => s.title)
    const data = await getResumeSuggestions(sectionsToRequest)
    
    resume.value.fullName = data.fullName || resume.value.fullName
    resume.value.email = data.email || resume.value.email
    
    const newSuggestions = {}
    let uniqueId = 1
    
    if (data.suggestions) {
      Object.keys(data.suggestions).forEach(key => {
        newSuggestions[key] = data.suggestions[key].map(t => ({ 
          id: `ai_${uniqueId++}_${Date.now()}`, 
          text: t 
        }))
      })
    }
    
    availableItems.value = newSuggestions
  }
  catch(err){
    console.error("AI Error:", err)
    alert("AI failed to generate suggestions: " + err.message)
  }
  finally{
    loading.value = false
    setTimeout(() => {
      document.querySelectorAll('.stylish-textarea').forEach(el => autoResize({target: el}))
    }, 100)
  }
}

onMounted(() => {
  refreshSuggestions()
})

function addSection() {
  resume.value.sections.push({
    id: "sec_" + Date.now(),
    title: "New Section",
    items: []
  })
}

function deleteSection(secIndex) {
  if(confirm("Are you sure you want to delete this section?")) {
    resume.value.sections.splice(secIndex, 1)
  }
}

function addBullet(secIndex) {
  resume.value.sections[secIndex].items.push({
    id: "item_" + Date.now(),
    text: ""
  })
}

function removeItem(secIndex, itemIndex){
  resume.value.sections[secIndex].items.splice(itemIndex, 1)
}

function autoResize(event) {
  const el = event.target || event
  if (!el) return
  el.style.height = 'auto'
  el.style.height = (el.scrollHeight) + 'px'
}

async function handleDownload(){
  try{
    // Make sure API call maps this correctly back to backend
    const blob = await downloadResumeFile(resume.value)
    const link = document.createElement("a")
    link.href = window.URL.createObjectURL(blob)
    link.download = `${resume.value.fullName.replace(/\s+/g, '_')}_Resume.docx`
    link.click()
  }catch(err){
    alert("Download failed: " + err.message)
  }
}
</script>

<style scoped>
.resume-wrapper{
  background: linear-gradient(135deg,#eef2ff,#f8fafc);
  min-height:100vh;
}

.gradient-text{
  background: linear-gradient(90deg,#4f46e5,#06b6d4);
  -webkit-background-clip:text;
  -webkit-text-fill-color:transparent;
}

.resume-card, .suggestion-card{
  border-radius:20px;
}

.suggestion-sticky {
  position: sticky;
  top: 2rem;
  z-index: 10;
}

@media (max-width: 991.98px) {
  .suggestion-sticky {
    position: static;
  }
}

.section-title-input {
  border: none;
  background: transparent;
  color: #334155;
  padding: 0;
}
.section-title-input:focus {
  outline: none;
  box-shadow: none;
  border-bottom: 2px solid #6366f1;
  border-radius: 0;
  background: transparent;
}

.drop-zone{
  min-height:90px;
  padding:15px;
  border:2px dashed #cbd5e1;
  border-radius:12px;
  background:white;
  transition:all 0.3s;
}

.drop-zone:hover{
  border-color:#6366f1;
  background:#f8fafc;
}

.resume-item{
  position:relative;
  margin-bottom:10px;
}

.stylish-textarea{
  border-radius:10px;
  resize:none;
  overflow: hidden;
  min-height: 40px;
}

.stylish-input{
  border-radius:10px;
}

.remove-btn{
  border:none;
  background:#ef4444;
  color:white;
  width:24px;
  height:24px;
  border-radius:50%;
  font-size:16px;
  cursor:pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.suggestion-item{
  background:#f1f5f9;
  padding:10px;
  border-radius:10px;
  margin-bottom:8px;
  cursor:grab;
  transition:all 0.2s;
}

.suggestion-item:hover{
  background:#e0e7ff;
  transform:translateY(-2px);
}

.download-btn{
  background:linear-gradient(90deg,#4f46e5,#06b6d4);
  border:none;
  border-radius:12px;
  font-weight:600;
  padding:12px;
  color:white;
  transition:0.3s;
}

.download-btn:hover{
  transform:scale(1.02);
  opacity:0.9;
}

.dashed-btn {
  border: 2px dashed #cbd5e1;
  background: transparent;
  color: #64748b;
  font-weight: 600;
}
.dashed-btn:hover {
  border-color: #6366f1;
  color: #6366f1;
  background: #f8fafc;
}
</style>
