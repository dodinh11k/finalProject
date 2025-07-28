// AI Chatbot with Google Gemini API
const chatBody = document.querySelector(".chat-body");
const messageInput = document.querySelector(".message-input");
const sendMessageButton = document.querySelector("#send-message");
const fileInput = document.querySelector("#file-input");
const fileUploadWrapper = document.querySelector(".file-upload-wrapper");
const fileCancelButton = document.querySelector("#file-cancel");
const chatbotToggler = document.querySelector("#chatbot-toggler");
const closeChatbot = document.querySelector("#close-chatbot");

// API setup
const API_KEY = "AIzaSyD0YOaN_TXkA8G9vhVutAy2G5FuWD5xEVY"; // Google Gemini API Key
const API_URL = `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=${API_KEY}`;

const userData = {
    message: null,
    file: {
        data: null,
        mime_type: null
    }
};

// System context for garage website
const chatHistory = [
    {
        role: "model",
        parts: [{ 
            text: `Tôi là AI Assistant của MyGarage - hệ thống đặt lịch bảo dưỡng và sửa chữa ô tô trực tuyến hàng đầu Việt Nam.

🚗 **Về MyGarage:**
- Chuyên cung cấp dịch vụ bảo dưỡng, sửa chữa ô tô chuyên nghiệp
- Hệ thống đặt lịch trực tuyến tiện lợi 24/7
- Đội ngũ kỹ thuật viên giàu kinh nghiệm
- Cam kết chất lượng và giá cả hợp lý

🔧 **Dịch vụ chính & Giá cả:**
1. **Thay dầu nhớt** - 200,000đ
   • Thay dầu nhờt động cơ và lọc dầu
   • Sử dụng dầu chất lượng cao
   
2. **Thay lốp xe** - 500,000đ  
   • Thay lốp xe và cân bằng bánh
   • Có lốp Michelin cao cấp - 800,000đ/cái
   
3. **Bảo dưỡng định kỳ** - 800,000đ
   • Kiểm tra và bảo dưỡng toàn bộ xe
   • Bảo trì tổng thể theo định kỳ
   
4. **Sửa chữa động cơ** - 1,500,000đ
   • Chẩn đoán và sửa chữa động cơ
   • Sử dụng phụ tùng chính hãng

🛒 **Sản phẩm chính:**
- Dầu nhớt 5W-30 chất lượng cao - 150,000đ
- Lọc dầu động cơ chính hãng - 50,000đ  
- Lốp xe Michelin cao cấp - 800,000đ
- Bộ phanh xe chất lượng - 300,000đ

📍 **Địa điểm:**
1. **Garage Trung Tâm** - 123 Đường ABC, Quận 1, TP.HCM
   • Phục vụ: Quận 1, Quận 3
   • Giờ làm việc: T2-T6: 8:00-18:00, T7: 8:00-17:00
   
2. **Garage Củ Chi** - 456 Đường XYZ, Huyện Củ Chi, TP.HCM  
   • Phục vụ: Huyện Củ Chi và vùng ngoại thành
   • Giờ làm việc: T2-T5: 7:00-19:00

💰 **Ưu đãi:**
- Mã giảm giá: SAVE10 (giảm 20k), TIRE20 (giảm 100k), OIL15 (giảm 30k), VIP20 (giảm 160k)
- Thanh toán trực tuyến an toàn qua PayOS
- Gửi email xác nhận tự động khi đặt lịch

📱 **Cách đặt lịch:**
1. Truy cập trang chủ → chọn "Đặt lịch hẹn"
2. Chọn dịch vụ cần thiết
3. Điền thông tin xe và thời gian mong muốn
4. Xác nhận và thanh toán
5. Nhận email xác nhận

❓ **Câu hỏi thường gặp:**
• "Xe tôi cần bảo dưỡng gì?" → Tư vấn dựa trên loại xe và km đã đi
• "Làm sao đặt lịch hẹn?" → Hướng dẫn từng bước đặt lịch
• "Có mã giảm giá nào không?" → Thông tin voucher hiện tại
• "Garage ở đâu?" → Địa chỉ và giờ làm việc 2 chi nhánh
• "Giá dịch vụ bao nhiêu?" → Bảng giá chi tiết tất cả dịch vụ

💬 **Tôi có thể hỗ trợ bạn:**
- Tư vấn dịch vụ phù hợp với xe (Honda, Toyota, BMW, Ford...)  
- Hướng dẫn đặt lịch hẹn từ A-Z
- Giải đáp thắc mắc về bảo dưỡng ô tô
- Thông tin giá cả, khuyến mãi và voucher
- Hỗ trợ kỹ thuật sử dụng website
- Tư vấn thời gian bảo dưỡng phù hợp

Hãy cho tôi biết xe bạn loại gì và cần hỗ trợ gì nhé! 🚗💙` 
        }],
    },
];
const initialInputHeight = messageInput ? messageInput.scrollHeight : 0;

// Create message element with dynamic classes and return it
const createMessageElement = (content, ...classes) => {
    const div = document.createElement("div");
    div.classList.add("message", ...classes);
    div.innerHTML = content;
    return div;
};

// Generate bot response using API
const generateBotResponse = async (incomingMessageDiv) => {
    const messageElement = incomingMessageDiv.querySelector(".message-text");
    
    // Enhanced user message with context
    const contextualMessage = `Với vai trò là AI Assistant của MyGarage - website đặt lịch sửa chữa ô tô, hãy trả lời câu hỏi sau một cách chuyên nghiệp và hữu ích: ${userData.message}`;
    
    chatHistory.push({
        role: "user",
        parts: [{ text: contextualMessage }, ...(userData.file.data ? [{ inline_data: userData.file }] : [])],
    });
    
    // API request options
    const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            contents: chatHistory
        })
    }

    try {
        // Fetch bot response from API
        const response = await fetch(API_URL, requestOptions);
        const data = await response.json();
        if (!response.ok) throw new Error(data.error.message);

        // Extract and display bot's response text
        const apiResponseText = data.candidates[0].content.parts[0].text.replace(/\*\*(.*?)\*\*/g, "$1").trim();
        messageElement.innerText = apiResponseText;
        chatHistory.push({
            role: "model",
            parts: [{ text: apiResponseText }]
        });
    } catch (error) {
        console.error("AI Chatbot Error:", error);
        messageElement.innerText = "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
        messageElement.style.color = "#ff0000";
    } finally {
        userData.file = {};
        incomingMessageDiv.classList.remove("thinking");
        if (chatBody) {
            chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
        }
    }
};

// Handle outgoing user message
const handleOutgoingMessage = (e) => {
    e.preventDefault();
    
    if (!messageInput) return;
    
    userData.message = messageInput.value.trim();
    if (!userData.message) return;
    
    messageInput.value = "";
    if (fileUploadWrapper) {
        fileUploadWrapper.classList.remove("file-uploaded");
    }
    messageInput.dispatchEvent(new Event("input"));

    // Create and display user message
    const messageContent = `<div class="message-text"></div>
                            ${userData.file.data ? `<img src="data:${userData.file.mime_type};base64,${userData.file.data}" class="attachment" />` : ""}`;

    const outgoingMessageDiv = createMessageElement(messageContent, "user-message");
    outgoingMessageDiv.querySelector(".message-text").innerText = userData.message;
    if (chatBody) {
        chatBody.appendChild(outgoingMessageDiv);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Simulate bot response with thinking indicator after a delay
    setTimeout(() => {
        const botMessageContent = `<span class="bot-avatar">🤖</span>
                <div class="message-text">
                    <div class="thinking-indicator">
                        <div class="dot"></div>
                        <div class="dot"></div>
                        <div class="dot"></div>
                    </div>
                </div>`;

        const incomingMessageDiv = createMessageElement(botMessageContent, "bot-message", "thinking");
        if (chatBody) {
            chatBody.appendChild(incomingMessageDiv);
            chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
        }
        generateBotResponse(incomingMessageDiv);
    }, 600);
};

// Initialize chatbot when DOM is loaded
document.addEventListener("DOMContentLoaded", function() {
    // Handle Enter key press for sending messages
    if (messageInput) {
        messageInput.addEventListener("keydown", (e) => {
            const userMessage = e.target.value.trim();
            if (e.key === "Enter" && userMessage && !e.shiftKey && window.innerWidth > 768) {
                handleOutgoingMessage(e);
            }
        });

        messageInput.addEventListener("input", (e) => {
            messageInput.style.height = `${initialInputHeight}px`;
            messageInput.style.height = `${messageInput.scrollHeight}px`;
            const chatForm = document.querySelector(".chat-form");
            if (chatForm) {
                chatForm.style.borderRadius = messageInput.scrollHeight > initialInputHeight ? "15px" : "32px";
            }
        });
    }

    // Handle file input change event
    if (fileInput) {
        fileInput.addEventListener("change", (e) => {
            const file = e.target.files[0];
            if (!file) return;
            
            // Validate file type
            const validImageTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
            if (!validImageTypes.includes(file.type)) {
                alert('Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WEBP)');
                fileInput.value = "";
                return;
            }
            
            const reader = new FileReader();
            reader.onload = (e) => {
                if (fileUploadWrapper) {
                    const img = fileUploadWrapper.querySelector("img");
                    if (img) {
                        img.src = e.target.result;
                        fileUploadWrapper.classList.add("file-uploaded");
                    }
                }
                const base64String = e.target.result.split(",")[1];
                userData.file = {
                    data: base64String,
                    mime_type: file.type
                };
            };
            reader.readAsDataURL(file);
        });
    }

    // Handle file cancel button
    if (fileCancelButton) {
        fileCancelButton.addEventListener("click", (e) => {
            userData.file = {};
            if (fileUploadWrapper) {
                fileUploadWrapper.classList.remove("file-uploaded");
            }
            if (fileInput) {
                fileInput.value = "";
            }
        });
    }

    // Handle send button click
    if (sendMessageButton) {
        sendMessageButton.addEventListener("click", handleOutgoingMessage);
    }

    // Handle file upload button
    const fileUploadButton = document.querySelector("#file-upload");
    if (fileUploadButton && fileInput) {
        fileUploadButton.addEventListener("click", () => fileInput.click());
    }

    // Handle chatbot toggler
    if (chatbotToggler) {
        chatbotToggler.addEventListener("click", () => {
            document.body.classList.toggle("show-chatbot");
        });
    }

    // Handle close chatbot
    if (closeChatbot) {
        closeChatbot.addEventListener("click", () => {
            document.body.classList.remove("show-chatbot");
        });
    }
}); 