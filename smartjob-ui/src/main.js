import { createApp } from "vue"
import App from "./App.vue"
import router from "./router"

// Bootstrap
import "bootstrap/dist/css/bootstrap.min.css"
import "bootstrap-icons/font/bootstrap-icons.css"
import "bootstrap"

// Global CSS
import "@/assets/main.css"

/* ===============================
   🔥 THEME INITIALIZATION (FIXED: ALWAYS WHITE)
   =============================== */

// Force white theme for all users
document.body.classList.remove("theme-dark")
document.body.classList.add("theme-light")

/* ===============================
   🚀 CREATE APP
   =============================== */

import { handleError } from "@/utils/error-handler"
 
 const app = createApp(App)
 app.config.errorHandler = (err) => handleError(err, 'Global Error')
 app.use(router).mount("#app")
