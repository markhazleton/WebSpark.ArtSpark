// Profile Edit Helper Script
(function () {
    'use strict';

    // Bio character counter
    const bioInput = document.getElementById('bio-input');
    const bioCounter = document.getElementById('bio-counter');

    if (bioInput && bioCounter) {
        bioInput.addEventListener('input', function () {
            const length = this.value.length;
            bioCounter.textContent = length;

            if (length > 450) {
                bioCounter.className = 'text-warning fw-bold';
            } else if (length === 500) {
                bioCounter.className = 'text-danger fw-bold';
            } else {
                bioCounter.className = '';
            }
        });
    }

    // Photo preview
    const newPhotoInput = document.getElementById('new-photo-input');
    const currentPhoto = document.getElementById('current-photo');
    const defaultAvatar = document.getElementById('default-avatar');
    const removePhotoCheck = document.getElementById('remove-photo-check');

    if (newPhotoInput) {
        newPhotoInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                // Validate file size (5MB max)
                if (file.size > 5 * 1024 * 1024) {
                    alert('File size must be less than 5MB');
                    e.target.value = '';
                    return;
                }

                // Validate file type
                if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
                    alert('Only JPEG, PNG, and WebP images are allowed');
                    e.target.value = '';
                    return;
                }

                // Show preview
                const reader = new FileReader();
                reader.onload = function (e) {
                    if (currentPhoto) {
                        currentPhoto.src = e.target.result;
                    } else if (defaultAvatar) {
                        // Replace default avatar with preview
                        const img = document.createElement('img');
                        img.src = e.target.result;
                        img.alt = 'New profile photo';
                        img.className = 'img-thumbnail rounded-circle';
                        img.style.width = '150px';
                        img.style.height = '150px';
                        img.style.objectFit = 'cover';
                        img.id = 'current-photo';
                        defaultAvatar.replaceWith(img);
                    }
                };
                reader.readAsDataURL(file);

                // Uncheck remove photo if user is uploading new one
                if (removePhotoCheck) {
                    removePhotoCheck.checked = false;
                }
            }
        });
    }

    // Disable new photo input when remove is checked
    if (removePhotoCheck && newPhotoInput) {
        removePhotoCheck.addEventListener('change', function () {
            if (this.checked) {
                newPhotoInput.value = '';
                newPhotoInput.disabled = true;
            } else {
                newPhotoInput.disabled = false;
            }
        });
    }
})();
