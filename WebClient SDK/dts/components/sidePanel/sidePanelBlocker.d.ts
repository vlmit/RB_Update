import * as React from 'react';
declare class SidePanelBlocker extends React.Component<SidePanelBlockerProps> {
    shouldComponentUpdate(nextProps: SidePanelBlockerProps): boolean;
    render(): JSX.Element;
}
export interface SidePanelBlockerProps {
    isPanelOpen: boolean;
}
export default SidePanelBlocker;
