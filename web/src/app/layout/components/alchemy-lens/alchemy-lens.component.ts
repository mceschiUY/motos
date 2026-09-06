import { Component, inject, signal, computed, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MutationStateService } from '../../../core/services/mutation-state.service';
import { SentinelService, GitDiffResult, FileDiff } from '../../../core/services/sentinel.service';

// ═══════════════════════════════════════════════════════════════════════════════
// ALCHEMY LENS COMPONENT
// Monaco Diff Editor para visualizar cambios git reales
// ═══════════════════════════════════════════════════════════════════════════════

declare const require: any;

@Component({
  selector: 'app-alchemy-lens',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './alchemy-lens.component.html',
  styleUrl: './alchemy-lens.component.scss'
})
export class AlchemyLensComponent implements AfterViewInit, OnDestroy {
  readonly state = inject(MutationStateService);
  private readonly sentinelService = inject(SentinelService);

  @ViewChild('monacoContainer') monacoContainer!: ElementRef<HTMLDivElement>;

  // Estado
  private readonly _isOpen = signal(false);
  private readonly _diffResult = signal<GitDiffResult | null>(null);
  private readonly _selectedFileIndex = signal(0);
  private readonly _loading = signal(false);
  private readonly _errorMsg = signal<string | null>(null);

  // Public
  readonly isOpen = this._isOpen.asReadonly();
  readonly diffResult = this._diffResult.asReadonly();
  readonly selectedFileIndex = this._selectedFileIndex.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly errorMsg = this._errorMsg.asReadonly();

  // Computed
  readonly shouldShow = computed(() => {
    return this.state.step() === 'done' ||
           (this.state.step() === 'executing' && this.state.streamingProgress() > 80);
  });

  readonly files = computed(() => this._diffResult()?.files ?? []);

  readonly selectedFile = computed<FileDiff | null>(() => {
    const f = this.files();
    const idx = this._selectedFileIndex();
    return f.length > idx ? f[idx] : null;
  });

  readonly branchName = computed(() => this._diffResult()?.branchName ?? '');
  readonly baseBranch = computed(() => this._diffResult()?.baseBranch ?? '');
  readonly totalAdditions = computed(() => this._diffResult()?.totalAdditions ?? 0);
  readonly totalDeletions = computed(() => this._diffResult()?.totalDeletions ?? 0);

  // Monaco
  private diffEditor: any = null;
  private monacoLoaded = false;
  private resizeObserver: ResizeObserver | null = null;

  ngAfterViewInit(): void {
    this.loadMonaco();
  }

  ngOnDestroy(): void {
    this.destroyEditor();
    this.resizeObserver?.disconnect();
  }

  // ═══════════════════════════════════════════════════════════════════════
  // ACTIONS
  // ═══════════════════════════════════════════════════════════════════════

  open(): void {
    this._isOpen.set(true);
    this.loadDiff();
  }

  close(): void {
    this._isOpen.set(false);
    this.destroyEditor();
  }

  toggle(): void {
    if (this._isOpen()) {
      this.close();
    } else {
      this.open();
    }
  }

  selectFile(index: number): void {
    this._selectedFileIndex.set(index);
    this.updateEditorContent();
  }

  refreshDiff(): void {
    this.loadDiff();
  }

  // ═══════════════════════════════════════════════════════════════════════
  // DIFF LOADING
  // ═══════════════════════════════════════════════════════════════════════

  private loadDiff(): void {
    this._loading.set(true);
    this._errorMsg.set(null);

    this.sentinelService.getDiff().subscribe({
      next: (result) => {
        this._diffResult.set(result);
        this._selectedFileIndex.set(0);
        this._loading.set(false);
        // Dar tiempo al DOM para renderizar
        setTimeout(() => this.initEditor(), 100);
      },
      error: (err) => {
        console.error('[AlchemyLens] Error loading diff:', err);
        this._errorMsg.set(err?.error?.error || err?.message || 'Error al obtener diff');
        this._loading.set(false);
      }
    });
  }

  // ═══════════════════════════════════════════════════════════════════════
  // MONACO EDITOR
  // ═══════════════════════════════════════════════════════════════════════

  private loadMonaco(): void {
    if (this.monacoLoaded) return;

    const onGotAmdLoader = () => {
      (window as any).require.config({ paths: { vs: 'assets/monaco-editor/min/vs' } });
      (window as any).require(['vs/editor/editor.main'], () => {
        this.monacoLoaded = true;
      });
    };

    if ((window as any).require) {
      onGotAmdLoader();
      return;
    }

    const script = document.createElement('script');
    script.src = 'assets/monaco-editor/min/vs/loader.js';
    script.onload = onGotAmdLoader;
    document.body.appendChild(script);
  }

  private initEditor(): void {
    if (!this.monacoContainer?.nativeElement) return;

    const tryInit = () => {
      const monaco = (window as any).monaco;
      if (!monaco) {
        // Reintentar si Monaco aún no cargó
        setTimeout(tryInit, 200);
        return;
      }

      this.destroyEditor();

      const container = this.monacoContainer.nativeElement;

      this.diffEditor = monaco.editor.createDiffEditor(container, {
        theme: 'vs-dark',
        readOnly: true,
        renderSideBySide: true,
        automaticLayout: false,
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        fontSize: 13,
        fontFamily: "'JetBrains Mono', 'Fira Code', monospace",
        lineNumbers: 'on',
        renderOverviewRuler: true,
        diffWordWrap: 'off'
      });

      // ResizeObserver para layout automático
      this.resizeObserver = new ResizeObserver(() => {
        this.diffEditor?.layout();
      });
      this.resizeObserver.observe(container);

      this.updateEditorContent();
    };

    tryInit();
  }

  private updateEditorContent(): void {
    const file = this.selectedFile();
    const monaco = (window as any).monaco;
    if (!this.diffEditor || !monaco || !file) return;

    const language = file.language || 'plaintext';

    const originalModel = monaco.editor.createModel(file.oldContent || '', language);
    const modifiedModel = monaco.editor.createModel(file.newContent || '', language);

    this.diffEditor.setModel({
      original: originalModel,
      modified: modifiedModel
    });
  }

  private destroyEditor(): void {
    if (this.diffEditor) {
      const model = this.diffEditor.getModel();
      if (model) {
        model.original?.dispose();
        model.modified?.dispose();
      }
      this.diffEditor.dispose();
      this.diffEditor = null;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // HELPERS
  // ═══════════════════════════════════════════════════════════════════════

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Created': return 'add_circle';
      case 'Modified': return 'edit';
      case 'Deleted': return 'remove_circle';
      default: return 'description';
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Created': return 'status-added';
      case 'Modified': return 'status-modified';
      case 'Deleted': return 'status-deleted';
      default: return '';
    }
  }

  getFileName(path: string): string {
    return path.split('/').pop() || path;
  }
}
