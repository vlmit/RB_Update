import * as React from 'react';
import { ElementSide, InteractionEventTypes } from 'common/utility';
declare class SidePanelWrapperProxy extends React.Component {
    render(): JSX.Element;
}
export interface SidePanelWrapperProps {
    openedElement: ElementSide | null;
    openEventType?: InteractionEventTypes.EVENT_TYPE_MOUSE | InteractionEventTypes.EVENT_TYPE_TOUCH;
    isLeftPanelOnClickOnly?: boolean;
    isRightPanelOnClickOnly?: boolean;
}
export default SidePanelWrapperProxy;
