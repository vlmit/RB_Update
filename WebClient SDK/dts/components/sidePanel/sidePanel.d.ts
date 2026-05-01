import * as React from 'react';
import { ElementSide } from 'common/utility';
export interface SidePanelProps {
    isPanelOpen: boolean;
    side: ElementSide.ELEMENT_LEFT_SIDE_MENU | ElementSide.ELEMENT_RIGHT_SIDE_MENU;
}
export default class SidePanel extends React.Component<SidePanelProps> {
    internalRef: any;
    private getTilePanel;
    private getTiles;
    render(): JSX.Element;
}
