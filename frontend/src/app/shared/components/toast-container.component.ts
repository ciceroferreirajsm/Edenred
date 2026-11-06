import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toasts" aria-live="polite">
      @for (t of toast.toasts(); track t.id) {
        <div class="toast toast-{{ t.tipo }}" role="status">
          <span>{{ t.mensagem }}</span>
          <button type="button" class="toast-close" aria-label="Fechar" (click)="toast.fechar(t.id)">×</button>
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  protected readonly toast = inject(ToastService);
}
