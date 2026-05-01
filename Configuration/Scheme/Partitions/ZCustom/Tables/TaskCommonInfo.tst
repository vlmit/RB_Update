<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="005962e7-65f1-4763-a0ef-b76751d26de3" Partition="d1b372f3-7565-4309-9037-5e5a0969d94e">
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4f9d8dd3-cd6e-445e-9677-85185c2640c1" Name="IsWaitingCompletion" Type="Boolean Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="9af956f6-308b-4756-b116-c19143562d56" Name="df_TaskCommonInfo_IsWaitingCompletion" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1795ab43-c568-4ab1-b49f-cff2d2788719" Name="Comment" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="5f9eac00-67cb-453b-b5f8-7d26686ac534" Name="ComDate" Type="Date Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a0681768-655c-4361-8e26-c6346ddb2002" Name="ParentTaskCardID" Type="Guid Null">
		<Description>Запись значения родительского поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="105dbb3b-10e9-4a7b-b1fb-e75488991bba" Name="TaskCardID" Type="Guid Null">
		<Description>Запись значения родительского поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6953e457-fa32-4f10-9a8c-b652abe3c587" Name="ChooseAllApprovers" Type="Boolean Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="1a1814d5-a486-4819-b672-eace85067fdf" Name="df_TaskCommonInfo_ChooseAllApprovers" Value="false" />
	</SchemePhysicalColumn>
</SchemeTable>