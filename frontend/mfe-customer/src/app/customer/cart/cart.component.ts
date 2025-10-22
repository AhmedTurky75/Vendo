import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface CartItem {
  id: string;
  name: string;
  price: number;
  quantity: number;
  image: string;
}

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent {
  cartItems: CartItem[] = [
    { id: '1', name: 'Premium Headphones', price: 199.99, quantity: 1, image: '🎧' },
    { id: '2', name: 'Wireless Mouse', price: 49.99, quantity: 2, image: '🖱️' }
  ];

  constructor(private router: Router) {}

  getSubtotal(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  }

  getTax(): number {
    return this.getSubtotal() * 0.1; // 10% tax
  }

  getTotal(): number {
    return this.getSubtotal() + this.getTax();
  }

  updateQuantity(item: CartItem, change: number): void {
    item.quantity = Math.max(1, item.quantity + change);
  }

  removeItem(itemId: string): void {
    this.cartItems = this.cartItems.filter(item => item.id !== itemId);
  }

  continueShopping(): void {
    this.router.navigate(['/']);
  }

  checkout(): void {
    console.log('Proceeding to checkout');
    // In real app, navigate to checkout
  }
}
