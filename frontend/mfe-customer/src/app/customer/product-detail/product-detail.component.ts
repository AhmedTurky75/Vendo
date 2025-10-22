import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {
  product = {
    id: '1',
    name: 'Premium Headphones',
    price: 199.99,
    image: '🎧',
    category: 'Electronics',
    rating: 4.5,
    description: 'High-quality wireless headphones with noise cancellation and premium sound quality. Perfect for music lovers and professionals.',
    features: [
      'Active Noise Cancellation',
      '30-hour battery life',
      'Premium sound quality',
      'Comfortable design',
      'Bluetooth 5.0'
    ],
    reviews: 128
  };

  quantity: number = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const productId = params['id'];
      // In real app, fetch product details by ID
      console.log('Loading product:', productId);
    });
  }

  incrementQuantity(): void {
    this.quantity++;
  }

  decrementQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  addToCart(): void {
    console.log(`Added ${this.quantity} x ${this.product.name} to cart`);
    // In real app, add to cart service
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
