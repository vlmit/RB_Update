<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="13b772ce-a56a-4e38-967a-89e1f25ff0a3" Name="ApprovalStages" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="13b772ce-a56a-0038-2000-09e1f25ff0a3" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="13b772ce-a56a-0138-4000-09e1f25ff0a3" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="13b772ce-a56a-0038-3100-09e1f25ff0a3" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="42377951-3910-42db-97ee-1cbb0967381b" Name="Order" Type="Int32 Not Null">
		<Description>Порядок этапа в списке</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="ac3550a3-96cd-418a-8de0-2dd6d01cebed" Name="df_ApprovalStages_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e76bd1c6-34fb-4e8c-8570-07e1af5580c1" Name="TimeLimit" Type="Double Null">
		<Description>Время на одно задание</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="44c7d29f-7ff8-4bec-a33e-e121ed80a843" Name="df_ApprovalStages_TimeLimit" Value="3" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="f27ee1bb-5676-45ed-9b80-fb7096c9bfe8" Name="Planned" Type="DateTime Null" />
	<SchemePhysicalColumn ID="6f79f946-0349-4770-85de-2e40051528ad" Name="IsParallel" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="5a575e5c-079e-47b8-9d1b-57c7bf6b88aa" Name="df_ApprovalStages_IsParallel" Value="true" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="effb6354-4318-43dc-a89f-5f03e7859273" Name="ReturnToAuthor" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="c36c7c8e-3835-461d-93b4-f021b43c6d06" Name="df_ApprovalStages_ReturnToAuthor" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="bc0e96df-cbd4-467e-a483-29d7bf5c482c" Name="ReturnWhenDisapproved" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="615a7f20-3451-44c8-b702-0c415bb25fe6" Name="df_ApprovalStages_ReturnWhenDisapproved" Value="true" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="45f415cc-7949-42a6-920d-51ac8b700c52" Name="Comment" Type="String(440) Null" />
	<SchemeComplexColumn ID="c1845220-4732-4e64-b97a-6557d63b6f53" Name="SubStageState" Type="Reference(Typified) Not Null" ReferencedTable="1c49f2d9-8688-4044-a7f5-7daedac250b2">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c1845220-4732-0064-4000-0557d63b6f53" Name="SubStageStateID" Type="Int16 Not Null" ReferencedColumn="3afc027c-da47-4ede-b398-80b4ee5928d9">
			<SchemeDefaultConstraint IsPermanent="true" ID="b6a5e437-55a8-49be-a5bb-d49e12fc2df6" Name="df_ApprovalStages_SubStageStateID" Value="0" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="cc6b0507-9c07-4b1a-848c-07adeed7b4cc" Name="SubStageStateName" Type="String(128) Not Null" ReferencedColumn="6b9087c0-b2a3-447f-9fa2-58486b2b3de0">
			<SchemeDefaultConstraint IsPermanent="true" ID="3f34a5f8-ad02-4bc1-b625-a2623739250a" Name="df_ApprovalStages_SubStageStateName" Value="Не запущен" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="e2011161-c866-457d-a04f-bc264b521df4" Name="DoNotReturnToApproved" Type="Boolean Not Null">
		<Description>Флаг не возвращать тем, кто согласовал ранее</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="d749ea9f-81c3-4dc2-b635-fea5361d0509" Name="df_ApprovalStages_DoNotReturnToApproved" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="5be4df5d-c27c-4917-bae4-808eef45fdf3" Name="TakeDelegatedAndAdditional" Type="Boolean Not Null">
		<Description>Флаг учитывать делегирования и доп. согласования</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="200fda0c-0bda-4c23-8ded-28cb95d7df58" Name="df_ApprovalStages_TakeDelegatedAndAdditional" Value="false" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="65e60a81-89a4-4a54-a82d-3d5bf1a10580" Name="StageType" Type="Reference(Typified) Not Null" ReferencedTable="57e7926b-8fad-4d24-bb1c-9d1d7f36a489">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="65e60a81-89a4-0054-4000-0d5bf1a10580" Name="StageTypeID" Type="Int16 Not Null" ReferencedColumn="9863c446-b64c-4875-b59a-d84508bff7b7">
			<SchemeDefaultConstraint IsPermanent="true" ID="0f3efa2d-710b-4621-b8b9-9e23707bda9e" Name="df_ApprovalStages_StageTypeID" Value="0" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="9c9f3b88-1d05-437e-b14e-bc6d51f98b1b" Name="StageTypeName" Type="String(128) Not Null" ReferencedColumn="6a4b1a82-b637-4ea8-82d3-9a49f1005ff4">
			<SchemeDefaultConstraint IsPermanent="true" ID="e05eabfc-4095-4b7a-8097-40381481a52b" Name="df_ApprovalStages_StageTypeName" Value="Согласование" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="5296bbae-6bc5-4201-9c81-1c00372468a9" Name="LeavePreviousStageDeadline" Type="Boolean Not Null">
		<Description>Флаг оставить прежний срок этапа</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="286d16ab-d84c-4dbf-8a7d-c2846b33e84d" Name="df_ApprovalStages_LeavePreviousStageDeadline" Value="false" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="13b772ce-a56a-0038-5000-09e1f25ff0a3" Name="pk_ApprovalStages">
		<SchemeIndexedColumn Column="13b772ce-a56a-0038-3100-09e1f25ff0a3" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="13b772ce-a56a-0038-7000-09e1f25ff0a3" Name="idx_ApprovalStages_ID" IsClustered="true">
		<SchemeIndexedColumn Column="13b772ce-a56a-0138-4000-09e1f25ff0a3" />
	</SchemeIndex>
</SchemeTable>