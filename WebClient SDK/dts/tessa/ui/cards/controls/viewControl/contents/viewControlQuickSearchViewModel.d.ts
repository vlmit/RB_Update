import type { ViewControlViewModel } from 'tessa/ui/cards/controls';
import { QuickSearchViewModel } from 'tessa/ui/views/content';
export declare class ViewControlQuickSearchViewModel extends QuickSearchViewModel<ViewControlViewModel> {
    constructor(viewControl: ViewControlViewModel);
    search(): void;
}
