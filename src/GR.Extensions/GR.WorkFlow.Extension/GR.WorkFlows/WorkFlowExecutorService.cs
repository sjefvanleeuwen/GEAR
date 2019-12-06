﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GR.Core.Attributes.Documentation;
using GR.Core.Extensions;
using GR.Core.Helpers;
using GR.Core.Helpers.Global;
using GR.Core.Helpers.Responses;
using GR.Identity.Abstractions;
using GR.WorkFlows.Abstractions;
using GR.WorkFlows.Abstractions.Helpers;
using GR.WorkFlows.Abstractions.Helpers.ActionHandlers;
using GR.WorkFlows.Abstractions.Models;
using GR.WorkFlows.Abstractions.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GR.WorkFlows
{
    [Author(Authors.LUPEI_NICOLAE)]
    [Documentation("Workflow executor flow")]
    public class WorkFlowExecutorService : IWorkFlowExecutorService
    {
        #region Injectable
        /// <summary>
        /// Inject workflow service
        /// </summary>

        private readonly IWorkFlowCreatorService<WorkFlow> _workFlowCreatorService;

        /// <summary>
        /// Inject workflow context
        /// </summary>
        private readonly IWorkFlowContext _workFlowContext;

        /// <summary>
        /// Inject user manager
        /// </summary>
        private readonly IUserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Inject logger
        /// </summary>
        private readonly ILogger<WorkFlowExecutorService> _logger;

        #endregion

        public WorkFlowExecutorService(IWorkFlowCreatorService<WorkFlow> workFlowCreatorService, IWorkFlowContext workFlowContext, IUserManager<ApplicationUser> userManager, ILogger<WorkFlowExecutorService> logger)
        {
            _workFlowCreatorService = workFlowCreatorService;
            _workFlowContext = workFlowContext;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Set start state for entry on all registered workflows
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel> SetStartStateForEntryAsync([Required]string entityName, [Required]string entryId)
        {
            var contractRequest = await GetEntityContractsAsync(entityName);
            if (!contractRequest.IsSuccess) return contractRequest.ToBase();
            var contracts = contractRequest.Result;
            foreach (var contract in contracts.Where(x => x.WorkFlow.Enabled).ToList())
            {
                var workflowRequest = await _workFlowCreatorService.GetWorkFlowByIdAsync(contract.WorkFlowId);
                if (!workflowRequest.IsSuccess) continue;
                var workflow = workflowRequest.Result;
                var startState = workflow.States.FirstOrDefault(x => x.IsStartState);
                if (startState == null) continue;
                await _workFlowContext.EntryStates.AddAsync(new EntryState
                {
                    EntryId = entryId,
                    StateId = startState.Id,
                    ContractId = contract.Id,
                });
            }

            return await _workFlowContext.PushAsync();
        }

        /// <summary>
        /// Change state for entry 
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="workFlowId"></param>
        /// <param name="newStateId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel> ChangeStateForEntryAsync([Required]string entryId, [Required] Guid? workFlowId, [Required] Guid? newStateId)
        {
            if (workFlowId == null || newStateId == null || entryId.IsNullOrEmpty()) return new InvalidParametersResultModel();
            var entryStateRequest = await GetEntryStateAsync(entryId, workFlowId);
            if (!entryStateRequest.IsSuccess) return entryStateRequest.ToBase();
            var entryState = entryStateRequest.Result;
            var transactionRequest = await _workFlowCreatorService.GetTransitionAsync(entryState.StateId, newStateId);
            if (!transactionRequest.IsSuccess) return transactionRequest.ToBase();
            var transaction = transactionRequest.Result;
            var userRequest = await _userManager.GetCurrentUserAsync();
            if (!userRequest.IsSuccess) return new NotAuthorizedResultModel().ToBase();
            var roles = await _userManager.GetUserRolesAsync(userRequest.Result);
            var allowPerformAction = roles.Select(x => x.Id.ToGuid()).ContainsAny(transaction.TransitionRoles.Select(x => x.RoleId));
            if (!allowPerformAction) return new ActionBlockedResultModel<object>().ToBase();

            entryState.StateId = newStateId.GetValueOrDefault();
            _workFlowContext.EntryStates.Update(entryState);
            var dbRequest = await _workFlowContext.PushAsync();
            if (dbRequest.IsSuccess) await ExecuteActionsAsync(transaction, new Dictionary<string, object>());
            return dbRequest;
        }

        /// <summary>
        /// Get next states for entry 
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="workFlowId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<StateGetViewModel>>> GetNextStatesForEntryAsync([Required]string entryId, [Required] Guid? workFlowId)
        {
            var entryStateRequest = await GetEntryStateAsync(entryId, workFlowId);
            if (!entryStateRequest.IsSuccess) return entryStateRequest.Map<IEnumerable<StateGetViewModel>>();
            var entryState = entryStateRequest.Result;
            var currentState = entryState.State;
            var nextStatesRequest = await GetNextStatesForAllowedRolesAsync(currentState);
            if (!nextStatesRequest.IsSuccess) return nextStatesRequest.Map<IEnumerable<StateGetViewModel>>();
            var mappedCollection = WorkFlowMapper.Map(nextStatesRequest.Result);
            return new SuccessResultModel<IEnumerable<StateGetViewModel>>(mappedCollection);
        }

        /// <summary>
        /// Get entity contracts
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<WorkFlowEntityContract>>> GetEntityContractsAsync([Required]string entityName)
        {
            if (entityName.IsNullOrEmpty()) return new InvalidParametersResultModel<IEnumerable<WorkFlowEntityContract>>();
            var contracts = await _workFlowContext.Contracts
                .Include(x => x.WorkFlow)
                .Where(x => x.EntityName.Equals(entityName))
                .ToListAsync();
            return new SuccessResultModel<IEnumerable<WorkFlowEntityContract>>(contracts);
        }

        /// <summary>
        /// Get entry state
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<EntryState>>> GetEntryStatesAsync([Required]string entryId)
        {
            if (entryId.IsNullOrEmpty()) return new InvalidParametersResultModel<IEnumerable<EntryState>>();
            var entry = await _workFlowContext.EntryStates
                .Include(x => x.State)
                .Include(x => x.Contract)
                .ThenInclude(x => x.WorkFlow)
                .Where(x => x.EntryId.Equals(entryId))
                .ToListAsync();
            if (entry == null) return new NotFoundResultModel<IEnumerable<EntryState>>();
            return new SuccessResultModel<IEnumerable<EntryState>>(entry);
        }

        /// <summary>
        /// Get entry state
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="workFlowId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<EntryState>> GetEntryStateAsync([Required]string entryId, [Required] Guid? workFlowId)
        {
            if (entryId.IsNullOrEmpty() || workFlowId == null) return new InvalidParametersResultModel<EntryState>();
            var entry = await _workFlowContext.EntryStates
                .Include(x => x.State)
                .Include(x => x.Contract)
                .ThenInclude(x => x.WorkFlow)
                .FirstOrDefaultAsync(x => x.EntryId.Equals(entryId) && x.Contract.WorkFlowId.Equals(workFlowId));
            if (entry == null) return new NotFoundResultModel<EntryState>();
            return new SuccessResultModel<EntryState>(entry);
        }

        /// <summary>
        /// Register entity contract
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="workFlowId"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<Guid>> RegisterEntityContractToWorkFlowAsync([Required]string entityName, Guid? workFlowId)
        {
            if (entityName.IsNullOrEmpty() || workFlowId == null) return new InvalidParametersResultModel<Guid>();
            var workFlowRequest = await _workFlowCreatorService.GetWorkFlowByIdAsync(workFlowId);
            if (!workFlowRequest.IsSuccess) return workFlowRequest.Map<Guid>();
            if (await IsAnyRegisteredContractToEntityAsync(entityName, workFlowId))
                return new InvalidParametersResultModel<Guid>("This workflow for the entity has already been recorded");
            var contract = new WorkFlowEntityContract
            {
                WorkFlowId = workFlowId.GetValueOrDefault(),
                EntityName = entityName
            };
            await _workFlowContext.Contracts.AddAsync(contract);
            var dbRequest = await _workFlowContext.PushAsync();
            return dbRequest.Map(contract.Id);
        }

        /// <summary>
        /// Remove entity contract
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="workFlowId"></param>
        /// <returns></returns>
        public async Task<ResultModel> RemoveEntityContractToWorkFlowAsync(string entityName, Guid? workFlowId)
        {
            if (entityName.IsNullOrEmpty() || workFlowId == null) return new InvalidParametersResultModel();
            var contract = await _workFlowContext.Contracts
                .AsNoTracking()
                .Include(x => x.WorkFlowId)
                .FirstOrDefaultAsync(x => x.EntityName.Equals(entityName) && x.WorkFlowId.Equals(workFlowId));
            if (contract == null) return new NotFoundResultModel();
            _workFlowContext.Contracts.Remove(contract);
            return await _workFlowContext.PushAsync();
        }

        /// <summary>
        /// Check for registered contract to entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="workFlowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsAnyRegisteredContractToEntityAsync([Required]string entityName, Guid? workFlowId)
            => await _workFlowContext.Contracts.AnyAsync(x => x.WorkFlowId.Equals(workFlowId) && x.EntityName.Equals(entityName));

        /// <summary>
        /// Force execute transition actions
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="transitionId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel> ForceExecuteTransitionActionsAsync(Guid? entryId, Guid? transitionId, Dictionary<string, object> data)
        {
            var entryState = await _workFlowContext.EntryStates
                .Include(x => x.Contract)
                .ThenInclude(x => x.WorkFlow)
                .FirstOrDefaultAsync(x => x.Id.Equals(entryId));
            if (entryState == null) return new InvalidParametersResultModel();
            var workFlowRequest = await _workFlowCreatorService.GetWorkFlowByIdAsync(entryState.Contract.WorkFlowId);
            if (!workFlowRequest.IsSuccess) return workFlowRequest.ToBase();
            var workFlow = workFlowRequest.Result;
            var transition = workFlow.Transitions.FirstOrDefault(x => x.Id.Equals(transitionId));
            await ExecuteActionsAsync(transition, data);
            return new SuccessResultModel<object>().ToBase();
        }

        /// <summary>
        /// Get roles for transition
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApplicationRole>> GetAllowedRolesToTransitionAsync(Transition transition)
        {
            Arg.NotNull(transition, nameof(GetAllowedRolesToTransitionAsync));
            var roleIds = transition?.TransitionRoles?.Select(x => x.RoleId).ToList() ?? new List<Guid>();
            var roles = await IoC.Resolve<IUserManager<ApplicationUser>>().FindRolesByIdAsync(roleIds);
            return roles.ToList();
        }

        /// <summary>
        /// Get next transitions
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<Transition>> GetNextTransitionsAsync(Transition transition)
        {
            Arg.NotNull(transition, nameof(GetNextTransitionsAsync));
            var nextState = transition.ToState;
            if (nextState == null) return new List<Transition>();
            var nextTransitions = await _workFlowContext.Transitions
                .Include(x => x.FromState)
                .Include(x => x.ToState)
                .Include(x => x.WorkFlow)
                .Include(x => x.TransitionRoles)
                .Include(x => x.TransitionActions)
                .Where(x => x.FromStateId.Equals(nextState.Id)).ToListAsync();

            return nextTransitions;
        }

        /// <summary>
        /// Get next transitions from state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<Transition>>> GetNextTransitionsFromStateAsync([Required]State state)
        {
            Arg.NotNull(state, nameof(GetNextTransitionsFromStateAsync));
            if (state == null) return new InvalidParametersResultModel<IEnumerable<Transition>>();
            var transitions = await _workFlowContext.Transitions
                .Include(x => x.FromState)
                .Include(x => x.ToState)
                .Include(x => x.TransitionRoles)
                .Where(x => x.FromStateId.Equals(state.Id))
                .ToListAsync();
            return new SuccessResultModel<IEnumerable<Transition>>(transitions);
        }

        /// <summary>
        /// Get next states from state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<State>>> GetNextStatesAsync([Required]State state)
        {
            Arg.NotNull(state, nameof(GetNextStatesAsync));
            var transitionsRequest = await GetNextTransitionsFromStateAsync(state);
            if (!transitionsRequest.IsSuccess) return transitionsRequest.Map<IEnumerable<State>>();
            var transitions = transitionsRequest.Result;
            var nextStates = transitions.Select(x => x.ToState).DistinctBy(x => x.Name).ToList();
            return new SuccessResultModel<IEnumerable<State>>(nextStates);
        }

        /// <summary>
        /// Get next states from state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual async Task<ResultModel<IEnumerable<State>>> GetNextStatesForAllowedRolesAsync([Required]State state)
        {
            Arg.NotNull(state, nameof(GetNextStatesAsync));
            var userRequest = await _userManager.GetCurrentUserAsync();
            if (!userRequest.IsSuccess) return userRequest.Map<IEnumerable<State>>();
            var transitionsRequest = await GetNextTransitionsFromStateAsync(state);
            if (!transitionsRequest.IsSuccess) return transitionsRequest.Map<IEnumerable<State>>();
            var transitions = transitionsRequest.Result.ToList();
            var userRoles = (await _userManager.GetUserRolesAsync(userRequest.Result)).ToList();
            var nextStates = transitions.Where(x => userRoles
                .Select(m => m.Id.ToGuid())
                .ContainsAny(x.TransitionRoles
                    .Select(y => y.RoleId))).Select(x => x.ToState).DistinctBy(x => x.Name).ToList();
            return new SuccessResultModel<IEnumerable<State>>(nextStates);
        }

        /// <summary>
        /// Execute actions
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task ExecuteActionsAsync(Transition transition, Dictionary<string, object> data)
        {
            var actions = transition.TransitionActions.Select(x => x.Action).ToList();
            var nextTransitions = await GetNextTransitionsAsync(transition);
            foreach (var action in actions)
            {
                try
                {
                    Type type = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var aType in assembly.GetTypes())
                        {
                            if (!aType.IsClass || aType.Name != action.ClassName) continue;
                            type = aType;
                            break;
                        }
                    }

                    if (type == null)
                    {
                        _logger.LogError($"Action {action.Name} was not found");
                        return;
                    }
                    var activatedObject = (BaseWorkFlowAction)Activator.CreateInstance(type, transition, nextTransitions);
                    if (activatedObject == null) return;
                    await activatedObject.InvokeExecuteAsync(data);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, e.Message);
                }
            }
        }
    }
}
