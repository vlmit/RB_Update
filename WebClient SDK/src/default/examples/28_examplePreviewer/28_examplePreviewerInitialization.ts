import { ApplicationExtension } from 'tessa';
import { ComponentRegistry } from 'tessa/ui/cards';
import { ExamplePreviewerViewModel } from './28_examplePreviewerViewModel';
import { createElement } from 'react';
import { ExamplePreviewerComponent } from './28_examplePreviewerComponent';

export class ExamplePreviewerInitialization extends ApplicationExtension {
  public initialize(): void {
    // регистрируем вью компонент и соответствующую ему вью-модель
    ComponentRegistry.instance.register(
      ExamplePreviewerViewModel.type,
      (viewModel: ExamplePreviewerViewModel) =>
        createElement(ExamplePreviewerComponent, { viewModel: viewModel }, null)
    );
  }
}
