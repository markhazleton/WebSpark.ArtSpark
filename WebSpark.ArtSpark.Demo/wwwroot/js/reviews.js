// Review and Rating functionality
document.addEventListener('DOMContentLoaded', function() {
    // Initialize rating stars
    initializeRatingStars();
    
    // Load reviews if on artwork details page
    const artworkId = getArtworkIdFromPage();
    if (artworkId) {
        loadReviews(artworkId);
        initializeFavoriteButton(artworkId);
    }

    // Initialize review form submission
    initializeReviewForm();
});

function initializeRatingStars() {
    const ratingStars = document.querySelectorAll('.rating-star');
    const ratingInputs = document.querySelectorAll('input[name="rating"]');
    
    ratingStars.forEach((star, index) => {
        star.addEventListener('click', function() {
            const rating = this.dataset.rating;
            
            // Update visual state
            updateStarDisplay(ratingStars, rating);
            
            // Update radio button
            if (ratingInputs[index]) {
                ratingInputs[index].checked = true;
            }
        });

        star.addEventListener('mouseenter', function() {
            const rating = this.dataset.rating;
            updateStarDisplay(ratingStars, rating, true);
        });
    });

    // Reset stars on mouse leave
    const ratingContainer = document.querySelector('.rating-input');
    if (ratingContainer) {
        ratingContainer.addEventListener('mouseleave', function() {
            const checkedRating = document.querySelector('input[name="rating"]:checked');
            const rating = checkedRating ? checkedRating.value : 0;
            updateStarDisplay(ratingStars, rating);
        });
    }
}

function updateStarDisplay(stars, rating, isHover = false) {
    stars.forEach((star, index) => {
        const starIcon = star.querySelector('i');
        const starRating = parseInt(star.dataset.rating);
        
        if (starRating <= rating) {
            starIcon.className = 'bi bi-star-fill';
            star.style.color = isHover ? '#ffc107' : '#ffc107';
        } else {
            starIcon.className = 'bi bi-star';
            star.style.color = '#6c757d';
        }
    });
}

function getArtworkIdFromPage() {
    // Extract artwork ID from URL or data attribute
    const urlParts = window.location.pathname.split('/');
    const detailsIndex = urlParts.indexOf('Details');
    
    if (detailsIndex !== -1 && urlParts[detailsIndex + 1]) {
        return parseInt(urlParts[detailsIndex + 1]);
    }
    
    return null;
}

async function loadReviews(artworkId) {
    try {
        const response = await fetch(`/Artwork/GetReviews?artworkId=${artworkId}`);
        const data = await response.json();
        
        if (data.error) {
            console.error('Error loading reviews:', data.error);
            return;
        }

        updateReviewSummary(data.averageRating, data.reviewCount);
        displayReviews(data.reviews);
        
    } catch (error) {
        console.error('Error loading reviews:', error);
        document.getElementById('reviews-container').innerHTML = 
            '<div class="alert alert-warning">Failed to load reviews.</div>';
    }
}

function updateReviewSummary(averageRating, reviewCount) {
    const averageElement = document.getElementById('average-rating');
    const countElement = document.getElementById('review-count');
    
    if (averageElement) {
        if (averageRating > 0) {
            averageElement.innerHTML = `
                <span class="text-warning">${'★'.repeat(Math.round(averageRating))}</span>
                <span class="text-muted">${'☆'.repeat(5 - Math.round(averageRating))}</span>
                <span class="ms-2">${averageRating}/5</span>
            `;
        } else {
            averageElement.textContent = 'No ratings yet';
        }
    }
    
    if (countElement) {
        countElement.textContent = `${reviewCount} review${reviewCount !== 1 ? 's' : ''}`;
    }
}

function displayReviews(reviews) {
    const container = document.getElementById('reviews-container');
    
    if (!reviews || reviews.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="bi bi-chat-dots display-6 mb-2"></i>
                <p>No reviews yet. Be the first to share your thoughts!</p>
            </div>
        `;
        return;
    }

    const reviewsHtml = reviews.map(review => {
        const profilePhoto = review.userPhotoUrl 
            ? `<img src="${review.userPhotoUrl}" alt="${escapeHtml(review.userName)} profile photo" class="rounded-circle me-2" style="width: 32px; height: 32px; object-fit: cover; border: 1px solid rgba(0,0,0,0.1);" />`
            : `<div class="rounded-circle bg-secondary d-inline-flex align-items-center justify-content-center me-2" style="width: 32px; height: 32px; color: white; font-size: 14px; font-weight: 500; border: 1px solid rgba(0,0,0,0.1);">${review.userInitial}</div>`;
        
        return `
            <div class="card mb-3 border-0 bg-body-secondary">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div class="d-flex align-items-start">
                            ${profilePhoto}
                            <div>
                                <h6 class="card-title mb-1">${escapeHtml(review.userName)}</h6>
                                <div class="text-warning mb-1">
                                    ${'★'.repeat(review.rating)}${'☆'.repeat(5 - review.rating)}
                                </div>
                            </div>
                        </div>
                        <div class="text-end">
                            <small class="text-muted">${review.createdAt}</small>
                            ${review.isOwner ? `
                                <button class="btn btn-sm btn-outline-danger ms-2" 
                                        onclick="deleteReview(${review.id})">
                                    <i class="bi bi-trash"></i>
                                </button>
                            ` : ''}
                        </div>
                    </div>
                    ${review.reviewText ? `
                        <p class="card-text ms-5 ps-2">${escapeHtml(review.reviewText)}</p>
                    ` : ''}
                </div>
            </div>
        `;
    }).join('');

    container.innerHTML = reviewsHtml;
}

function initializeReviewForm() {
    const reviewForm = document.getElementById('review-form');
    if (!reviewForm) return;

    reviewForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const formData = new FormData(this);
        const rating = formData.get('rating');
        
        if (!rating) {
            alert('Please select a rating before submitting your review.');
            return;
        }

        const submitButton = this.querySelector('button[type="submit"]');
        const originalText = submitButton.innerHTML;
        
        // Show loading state
        submitButton.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Submitting...';
        submitButton.disabled = true;

        try {
            const response = await fetch(this.action, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                // Reload the page to show the updated review
                window.location.reload();
            } else {
                throw new Error('Failed to submit review');
            }
        } catch (error) {
            console.error('Error submitting review:', error);
            alert('Failed to submit review. Please try again.');
        } finally {
            submitButton.innerHTML = originalText;
            submitButton.disabled = false;
        }
    });
}

function initializeFavoriteButton(artworkId) {
    const favoriteBtn = document.getElementById('favorite-btn');
    if (!favoriteBtn) return;

    favoriteBtn.addEventListener('click', async function() {
        const originalHtml = this.innerHTML;
        this.innerHTML = '<i class="spinner-border spinner-border-sm"></i> Processing...';
        this.disabled = true;

        try {
            const response = await fetch('/Artwork/ToggleFavorite', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `artworkId=${artworkId}`
            });

            const data = await response.json();
            
            if (data.success) {
                if (data.isFavorited) {
                    this.innerHTML = '<i class="bi bi-heart-fill"></i> <span>Remove from Favorites</span>';
                    this.classList.remove('btn-outline-danger');
                    this.classList.add('btn-danger');
                } else {
                    this.innerHTML = '<i class="bi bi-heart"></i> <span>Add to Favorites</span>';
                    this.classList.remove('btn-danger');
                    this.classList.add('btn-outline-danger');
                }
            } else {
                throw new Error(data.error || 'Failed to update favorite status');
            }
        } catch (error) {
            console.error('Error toggling favorite:', error);
            alert('Failed to update favorite status. Please try again.');
            this.innerHTML = originalHtml;
        } finally {
            this.disabled = false;
        }
    });
}

async function deleteReview(reviewId) {
    if (!confirm('Are you sure you want to delete this review?')) {
        return;
    }

    try {
        const response = await fetch(`/Artwork/DeleteReview/${reviewId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        const data = await response.json();
        
        if (data.success) {
            // Reload reviews
            const artworkId = getArtworkIdFromPage();
            if (artworkId) {
                loadReviews(artworkId);
            }
        } else {
            throw new Error(data.error || 'Failed to delete review');
        }
    } catch (error) {
        console.error('Error deleting review:', error);
        alert('Failed to delete review. Please try again.');
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
