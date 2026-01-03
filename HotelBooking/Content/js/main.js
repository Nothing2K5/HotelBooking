(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($("#spinner").length > 0) {
                $("#spinner").removeClass("show");
            }
        }, 1);
    };
    spinner();

    // WOW animation
    new WOW().init();

    // Dropdown hover fix
    const $dropdown = $(".dropdown");
    const $dropdownToggle = $(".dropdown-toggle");
    const $dropdownMenu = $(".dropdown-menu");
    const showClass = "show";

    $(window).on("load resize", function () {
        if (this.matchMedia("(min-width: 992px)").matches) {
            $dropdown.hover(
                function () {
                    const $this = $(this);
                    $this.addClass(showClass);
                    $this.find($dropdownToggle).attr("aria-expanded", "true");
                    $this.find($dropdownMenu).addClass(showClass);
                },
                function () {
                    const $this = $(this);
                    $this.removeClass(showClass);
                    $this.find($dropdownToggle).attr("aria-expanded", "false");
                    $this.find($dropdownMenu).removeClass(showClass);
                }
            );
        } else {
            $dropdown.off("mouseenter mouseleave");
        }
    });

    // Back to top
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $(".back-to-top").fadeIn("slow");
        } else {
            $(".back-to-top").fadeOut("slow");
        }
    });
    $(".back-to-top").click(function () {
        $("html, body").animate({ scrollTop: 0 }, 1500, "easeInOutExpo");
        return false;
    });

    // Counter
    $("[data-toggle='counter-up']").counterUp({ delay: 10, time: 2000 });

    // Booking Form - Flatpickr
    const checkinPicker = flatpickr("#checkin", {
        dateFormat: "d/m/Y",
        minDate: "today",
        locale: {
            firstDayOfWeek: 1
        },
        onChange: function (selectedDates, dateStr) {
            checkoutPicker.set("minDate", dateStr);
        }
    });

    const checkoutPicker = flatpickr("#checkout", {
        dateFormat: "d/m/Y",
        minDate: "today",
        locale: {
            firstDayOfWeek: 1
        }
    });
    // Guest dropdown logic
    const popup = $(".guest-popup");
    const toggle = $("#guestDropdownToggle");
    const summary = $("#guestSummary");
    const childrenAges = $("#childrenAges");

    toggle.on("click", function (e) {
        e.stopPropagation();
        popup.toggleClass("show");
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest(".guest-popup, #guestDropdownToggle").length) {
            popup.removeClass("show");
        }
    });

    function updateSummary() {
        const rooms = $("#roomCount").text();
        const adults = $("#adultCount").text();
        const children = $("#childCount").text();
        let text = `${rooms} phòng • ${adults} người lớn`;
        if (children > 0) {
            const ages = [];
            $(".child-age").each(function () {
                ages.push($(this).val() || "?");
            });
            text += ` • ${children} trẻ (${ages.join(", ")} tuổi)`;
        }
        summary.val(text);
    }

    // Button +/-
    $(".guest-popup").on("click", ".btn-plus, .btn-minus", function () {
        const countEl = $(this).siblings(".count");
        let count = parseInt(countEl.text());
        if ($(this).hasClass("btn-plus")) count++;
        else if (count > 0) count--;

        countEl.text(count);

        // Children age input
        const childCount = parseInt($("#childCount").text());
        childrenAges.empty();
        for (let i = 1; i <= childCount; i++) {
            childrenAges.append(
                `<div class="d-flex align-items-center mb-2">
            <label class="me-2 mb-0 small">Tuổi trẻ ${i}:</label>
            <select class="form-select form-select-sm w-auto child-age">
              ${Array.from({ length: 18 }, (_, a) => `<option>${a}</option>`).join("")}
            </select>
          </div>`
            );
        }

        updateSummary();
    });
})(jQuery);

// ==================== SEARCH RESULTS: FILTER & SORT ====================

if (window.location.pathname.includes("search-results.html")) {
    // Dữ liệu phòng (mô phỏng thực tế)
    const hotels = [
        { id: 1, name: "ABC Sunshine Hotel", district: "Quận 1", rating: 9.2, reviews: 1245, room: "Deluxe", bed: "1 giường king", size: "35m²", price: 2880000, oldPrice: 3200000, freeCancel: true, breakfast: true, pool: true, spa: true, img: "img/hotel-1.jpg" },
        { id: 2, name: "ABC Paradise Resort", district: "Quận 7", rating: 8.7, reviews: 892, room: "Standard", bed: "1 giường đôi", size: "28m²", price: 1980000, oldPrice: 0, freeCancel: false, breakfast: true, pool: true, spa: false, img: "img/hotel-2.jpg" },
        { id: 3, name: "ABC Luxury Suites", district: "Quận 3", rating: 9.5, reviews: 567, room: "Suite", bed: "2 giường", size: "50m²", price: 4500000, oldPrice: 6000000, freeCancel: true, breakfast: true, pool: false, spa: true, img: "img/hotel-3.jpg" },

    ];

    const checkin = "16/11/2025", checkout = "18/11/2025";

    // Render 1 khách sạn
    function renderHotel(h) {
        return `
      <div class="col-12 wow fadeInUp hotel-item" data-wow-delay="0.1s"
           data-price="${h.price}" data-rating="${h.rating}"
           data-free-cancel="${h.freeCancel}" data-breakfast="${h.breakfast}"
           data-pool="${h.pool}" data-spa="${h.spa}">
        <div class="hotel-card card border-0 shadow-sm">
          <div class="row g-0">
            <div class="col-md-4 position-relative">
              <img src="${h.img}" class="img-fluid" style="height: 220px; object-fit: cover;">
              ${h.freeCancel ? '<div class="position-absolute top-0 end-0 m-2"><span class="badge bg-success text-white">Miễn phí hủy</span></div>' : ''}
              ${h.oldPrice > 0 ? `<div class="position-absolute top-0 start-0 m-2"><span class="badge bg-danger text-white">-${Math.round((1 - h.price / h.oldPrice) * 100)}%</span></div>` : ''}
            </div>
            <div class="col-md-8">
              <div class="card-body d-flex flex-column h-100">
                <div class="d-flex justify-content-between align-items-start mb-2">
                  <div>
                    <h5 class="mb-1">${h.name}</h5>
                    <p class="text-muted small mb-1"><i class="fa fa-map-marker-alt text-primary me-1"></i>${h.district}, TP.HCM</p>
                    <div class="d-flex gap-1 align-items-center">
                      <div class="rating-box">${h.rating}</div>
                      <small class="text-muted ms-1">(${h.reviews} đánh giá)</small>
                    </div>
                  </div>
                </div>
                <p class="text-body small mb-2">Phòng ${h.room} • ${h.bed} • ${h.size}</p>
                <div class="d-flex gap-2 flex-wrap mb-3">
                  ${h.breakfast ? '<span class="badge bg-light text-dark">Bữa sáng</span>' : ''}
                  ${h.pool ? '<span class="badge bg-light text-dark">Hồ bơi</span>' : ''}
                  ${h.spa ? '<span class="badge bg-light text-dark">Spa</span>' : ''}
                  <span class="badge bg-light text-dark">Wifi</span>
                </div>
                <div class="mt-auto d-flex justify-content-between align-items-end">
                  <div>
                    ${h.oldPrice > 0 ? `<del class="price-strike text-muted">${h.oldPrice.toLocaleString()}đ</del>` : ''}
                    <div class="price-main">${h.price.toLocaleString()}đ</div>
                    <small class="text-muted">2 đêm • đã gồm thuế</small>
                  </div>
                  <a href="booking-step.html?hotel=${h.id}&room=${h.room}&checkin=${checkin}&checkout=${checkout}" 
                     class="btn btn-dark px-4">Chọn phòng</a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
    }

    // Lọc & sắp xếp
    function filterAndSort() {
        let filtered = [...hotels];

        // Lọc giá
        const maxPrice = parseInt($("#priceRange").val());
        filtered = filtered.filter(h => h.price <= maxPrice);

        // Lọc tiện ích
        $(".filter-btn.active").each(function () {
            const filter = $(this).data("filter");
            filtered = filtered.filter(h => h[filter]);
        });

        // Lọc sao
        const stars = [];
        $(".filter-star:checked").each(function () {
            stars.push(parseInt($(this).data("star")));
        });
        if (stars.length > 0) {
            filtered = filtered.filter(h => stars.includes(Math.floor(h.rating)));
        }

        // Sắp xếp
        const sort = $("#sortSelect").val();
        if (sort === "price_asc") filtered.sort((a, b) => a.price - b.price);
        else if (sort === "price_desc") filtered.sort((a, b) => b.price - a.price);
        else if (sort === "rating_desc") filtered.sort((a, b) => b.rating - a.rating);

        // Render
        const html = filtered.map(renderHotel).join("");
        $("#resultList").html(html || '<div class="col-12 text-center py-5"><p class="text-muted">Không tìm thấy khách sạn nào.</p></div>');
    }

    // Sự kiện
    $("#priceRange, #sortSelect").on("input change", filterAndSort);
    $(document).on("click", ".filter-btn", function () {
        $(this).toggleClass("active bg-primary text-white");
        filterAndSort();
    });
    $(document).on("change", ".filter-star", filterAndSort);

    // Khởi tạo
    filterAndSort();

    // Cập nhật giá
    $("#priceRange").on("input", function () {
        $("#priceValue").text(parseInt($(this).val()).toLocaleString() + "đ");
    });
}
// ==================== HISTORY-DETAIL.HTML: CHI TIẾT BOOKING ====================

if (window.location.pathname.includes("history-detail.html")) {
    // Lấy ID từ URL: history-detail.html?id=123
    const urlParams = new URLSearchParams(window.location.search);
    const bookingId = urlParams.get('id') || "123";

    // Dữ liệu mẫu (sẽ thay bằng API sau)
    const bookings = {
        "123": {
            code: "ABC123456",
            status: "confirmed",
            bookedDate: "15/11/2025",
            hotel: { name: "ABC Sunshine Hotel", address: "Quận 1, TP.HCM", img: "img/hotel-1.jpg" },
            room: "Deluxe",
            checkin: "16/11/2025",
            checkout: "18/11/2025",
            guest: { name: "My Nguyễn", email: "my.nguyen@example.com", phone: "0901234567" },
            price: 5760000,
            payment: "Thẻ tín dụng"
        }
    };

    const b = bookings[bookingId] || bookings["123"];

    // Hiển thị dữ liệu
    $("#bookingCode").text(b.code);
    $("#bookedDate").text(b.bookedDate);
    $("#statusBadge").text(b.status === "confirmed" ? "Đã xác nhận" : "Đã hủy")
        .removeClass("bg-success bg-danger")
        .addClass(b.status === "confirmed" ? "bg-success" : "bg-danger");
    $("#hotelName").text(b.hotel.name);
    $("#hotelAddress").text(b.hotel.address);
    $("#roomType").text(b.room);
    $("#checkinDate").text(b.checkin);
    $("#checkoutDate").text(b.checkout);
    $("#guestName").text(b.guest.name);
    $("#guestEmail").text(b.guest.email);
    $("#guestPhone").text(b.guest.phone);
    $("#roomPrice").text(b.price.toLocaleString() + "đ");
    $("#totalAmount").text(b.price.toLocaleString() + "đ");

    // Cập nhật hình ảnh
    $("#hotelSummary img, .info-card img").attr("src", b.hotel.img);
}
// ==================== HISTORY-DETAIL.HTML: CHI TIẾT BOOKING + NÚT ĐÁNH GIÁ ====================

if (window.location.pathname.includes("history-detail.html")) {
    const urlParams = new URLSearchParams(window.location.search);
    const bookingId = urlParams.get('id') || "123";

    // Dữ liệu mẫu (có trạng thái hoàn thành để hiện nút đánh giá)
    const bookings = {
        "123": {
            code: "ABC123456",
            status: "completed", // completed = đã trả phòng → được đánh giá
            bookedDate: "15/11/2025",
            hotel: { name: "ABC Sunshine Hotel", address: "Quận 1, TP.HCM", img: "img/hotel-1.jpg" },
            room: "Deluxe",
            checkin: "16/11/2025",
            checkout: "18/11/2025",
            guest: { name: "My Nguyễn", email: "my.nguyen@example.com", phone: "0901234567" },
            price: 5760000,
            payment: "Thẻ tín dụng"
        },
        "456": {
            code: "ABC456789",
            status: "confirmed", // đang ở → chưa đánh giá
            bookedDate: "10/11/2025",
            hotel: { name: "ABC Paradise Resort", address: "Quận 7, TP.HCM", img: "img/hotel-2.jpg" },
            room: "Standard",
            checkin: "14/11/2025",
            checkout: "16/11/2025",
            guest: { name: "My Nguyễn", email: "my.nguyen@example.com", phone: "0901234567" },
            price: 3960000,
            payment: "MoMo"
        }
    };

    const b = bookings[bookingId] || bookings["123"];

    // Hiển thị thông tin
    $("#bookingCode").text(b.code);
    $("#bookedDate").text(b.bookedDate);

    // Cập nhật trạng thái
    const statusText = b.status === "completed" ? "Đã hoàn thành" :
        b.status === "confirmed" ? "Đã xác nhận" : "Đã hủy";
    const statusClass = b.status === "completed" ? "bg-success" :
        b.status === "confirmed" ? "bg-primary" : "bg-danger";
    $("#statusBadge").text(statusText).removeClass("bg-success bg-primary bg-danger").addClass(statusClass);

    $("#hotelName").text(b.hotel.name);
    $("#hotelAddress").text(b.hotel.address);
    $("#roomType").text(b.room);
    $("#checkinDate").text(b.checkin);
    $("#checkoutDate").text(b.checkout);
    $("#guestName").text(b.guest.name);
    $("#guestEmail").text(b.guest.email);
    $("#guestPhone").text(b.guest.phone);
    $("#roomPrice").text(b.price.toLocaleString() + "đ");
    $("#totalAmount").text(b.price.toLocaleString() + "đ");
    $("img").attr("src", b.hotel.img);

    // HIỂN THỊ NÚT ĐÁNH GIÁ NẾU ĐÃ HOÀN THÀNH
    if (b.status === "completed") {
        $(`#reviewBtn`)
            .removeClass("d-none")
            .attr("href", `review.html?bookingId=${bookingId}`);
    }

    // Cập nhật timeline
    $(".timeline-item").removeClass("completed");
    if (b.status === "completed" || b.status === "confirmed") {
        $(".timeline-item").eq(0).addClass("completed"); // Đã đặt
        $(".timeline-item").eq(1).addClass("completed"); // Thanh toán
    }
    if (b.status === "completed") {
        $(".timeline-item").eq(2).addClass("completed"); // Nhận phòng
        $(".timeline-item").eq(3).addClass("completed"); // Trả phòng
    }
}
// ==================== REVIEW.HTML: VIẾT ĐÁNH GIÁ ====================

// REVIEW.HTML - Xử lý sao + thông báo

if (window.location.pathname.includes("review.html")) {
    let selectedRating = 0;

    // Sao lớn tổng thể
    $("#overallRating i").on("click", function () {
        selectedRating = $(this).data("value");
        $("#overallRating i").removeClass("filled");
        $(this).prevAll().addBack().addClass("filled");

        const texts = ["", "Rất tệ", "Tệ", "Bình thường", "Tốt", "Tuyệt vời!"];
        $("#ratingText")
            .text(texts[selectedRating])
            .removeClass("text-muted")
            .addClass("text-success fw-bold");
    });

    // Hover sao lớn
    $("#overallRating i").hover(
        function () { $(this).prevAll().addBack().addClass("hovered"); },
        function () { $("#overallRating i").removeClass("hovered"); }
    );

    // Sao nhỏ nhỏ (4 tiêu chí)
    $(".star-rating-sm i").on("click", function () {
        const val = $(this).data("value");
        $(this).parent().find("i").removeClass("filled");
        $(this).prevAll().addBack().addClass("filled");
    });

    // Hover sao nhỏ
    $(".star-rating-sm i").hover(
        function () { $(this).prevAll().addBack().addClass("hovered"); },
        function () { $(this).parent().find("i").removeClass("hovered"); }
    );

    // Submit
    $("#reviewForm").on("submit", function (e) {
        e.preventDefault();
        if (selectedRating === 0) {
            alert("Vui lòng chọn số sao tổng thể!");
            return;
        }
        alert("🎉 Cảm ơn bạn đã đánh giá!\n+50 điểm đã được cộng.");
        window.location.href = "history.html";
    });
}
