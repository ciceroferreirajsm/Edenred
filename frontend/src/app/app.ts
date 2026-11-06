import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ToastContainerComponent } from './shared/components/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastContainerComponent],
  templateUrl: './app.html',
})
export class App {}
