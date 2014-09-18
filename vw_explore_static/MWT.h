//
// Main interface for clients to the MWT service.
//

#include "stdafx.h"
#include <typeinfo>
#include "hash.h"

class BaseFunctionWrapper { };
class MWT_Empty { };

typedef u32 Stateful_Policy_Func(void* state_Context, void* application_Context);
typedef u32 Stateless_Policy_Func(void* application_Context);

template <class T>
class StatefulFunctionWrapper : public BaseFunctionWrapper
{
public:
	typedef u32 Policy_Func(T* state_Context, Context* application_Context);

	Stateful_Policy_Func* m_policy_function;
};

class StatelessFunctionWrapper : public BaseFunctionWrapper
{
public:
	typedef u32 Policy_Func(Context* application_Context);

	Stateless_Policy_Func* m_policy_function;
};

// TODO: for exploration budget, exploration algo should implement smth like Start & Stop Explore, Adjust epsilon
class Explorer : public Policy
{
};

template <class T>
class EpsilonGreedyExplorer : public Explorer
{
public:
	EpsilonGreedyExplorer(
		float epsilon, 
		BaseFunctionWrapper& default_policy_func_wrapper, 
		T* default_policy_func_state_context) :
			m_epsilon(epsilon),
			m_default_policy_wrapper(default_policy_func_wrapper),
			m_default_policy_state_context(default_policy_func_state_context)
	{
		if (epsilon <= 0)
		{
			throw std::invalid_argument("Initial epsilon value must be positive.");
		}
		m_random_generator = new PRG<u32>();
	}

	~EpsilonGreedyExplorer()
	{
		delete m_random_generator;
	}

	std::pair<MWTAction, float> Choose_Action(Context& context, ActionSet& actions)
	{
		return this->Choose_Action(context, actions, *m_random_generator);
	}

	std::pair<MWTAction, float> Choose_Action(Context& context, ActionSet& actions, u32 seed)
	{
		PRG<u32> random_generator(seed);
		return this->Choose_Action(context, actions, random_generator);
	}

private:
	std::pair<MWTAction, float> Choose_Action(Context& context, ActionSet& actions, PRG<u32>& random_generator)
	{
		// Invoke the default policy function to get the action
		MWTAction chosen_action(0);
		if (typeid(m_default_policy_wrapper) == typeid(StatelessFunctionWrapper))
		{
			StatelessFunctionWrapper* stateless_function_wrapper = (StatelessFunctionWrapper*)(&m_default_policy_wrapper);
			chosen_action = MWTAction(stateless_function_wrapper->m_policy_function(&context));
		}
		else
		{
			StatefulFunctionWrapper<T>* stateful_function_wrapper = (StatefulFunctionWrapper<T>*)(&m_default_policy_wrapper);
			chosen_action = MWTAction(stateful_function_wrapper->m_policy_function(m_default_policy_state_context, &context));
		}

		float action_probability = 0.f;
		float base_probability = m_epsilon / actions.Count(); // uniform probability
		
		// TODO: check this random generation
		if (((double)random_generator.Uniform_Int() / (2e32 - 1)) < 1.f - m_epsilon)
		{
			action_probability = 1.f - m_epsilon + base_probability;
		}
		else
		{
			// Get uniform random action ID
			u32 actionId = random_generator.Uniform_Int(1, actions.Count());

			if (actionId == chosen_action.Get_Id())
			{
				// IF it matches the one chosen by the default policy
				// then increase the probability
				action_probability = 1.f - m_epsilon + base_probability;
			}
			else
			{
				// Otherwise it's just the uniform probability
				action_probability = base_probability;
			}
			chosen_action = actions.Get(actionId);
		}

		return std::pair<MWTAction, float>(chosen_action, action_probability);
	}

private:
	float m_epsilon;
	PRG<u32>* m_random_generator;

	BaseFunctionWrapper& m_default_policy_wrapper;
	T* m_default_policy_state_context;
};

class MWT
{
public:
	MWT(std::string& app_id, u32 num_actions) : m_app_id(app_id)
	{
		IdGenerator::Initialize();

		if (m_app_id.empty())
		{
			m_app_id = this->Generate_App_Id();
		}

		m_logger = new Logger(m_app_id);
		m_action_set = new ActionSet(num_actions);
	}

	~MWT()
	{
		IdGenerator::Destroy();

		delete m_logger;
		delete m_explorer;
		delete m_action_set;
	}

	template <class T>
	void Initialize_Epsilon_Greedy(
		float epsilon, 
		typename StatefulFunctionWrapper<T>::Policy_Func defaultPolicyFunc, 
		T* defaultPolicyFuncStateContext)
	{
		StatefulFunctionWrapper<T>* func_Wrapper = new StatefulFunctionWrapper<T>();
		func_Wrapper->m_policy_function = (Stateful_Policy_Func*)&defaultPolicyFunc;
		
		m_explorer = new EpsilonGreedyExplorer<T>(epsilon, *func_Wrapper, defaultPolicyFuncStateContext);
		
		m_default_func_wrapper = func_Wrapper;
	}

	void Initialize_Epsilon_Greedy(
		float epsilon, 
		StatelessFunctionWrapper::Policy_Func default_Policy_Func)
	{
		this->Initialize_Epsilon_Greedy(epsilon, (Stateless_Policy_Func*)default_Policy_Func);
	}

	std::pair<u32, u64> Choose_Action_Join_Key(Context& context)
	{
		std::pair<MWTAction, float> action_Probability_Pair = m_explorer->Choose_Action(context, *m_action_set);
		Interaction pInteraction(&context, action_Probability_Pair.first, action_Probability_Pair.second);
		m_logger->Store(&pInteraction);
		
		// TODO: Anything else to do here?

		return std::pair<u32, u64>(action_Probability_Pair.first.Get_Id(), pInteraction.Get_Id());
	}

	// TODO: check whether char* could be std::string
	u32 Choose_Action(Context& context, char* unique_id, u32 length)
	{
		u32 seed = this->Compute_Seed(unique_id, length);

		std::pair<MWTAction, float> action_Probability_Pair = m_explorer->Choose_Action(context, *m_action_set, seed);
		Interaction pInteraction(&context, action_Probability_Pair.first, action_Probability_Pair.second, seed);
		m_logger->Store(&pInteraction);

		// TODO: Anything else to do here?

		return action_Probability_Pair.first.Get_Id();
	}

// Cross-language interface
public:
	void Initialize_Epsilon_Greedy(
		float epsilon, 
		Stateful_Policy_Func default_policy_func, 
		void* default_policy_func_argument)
	{
		StatefulFunctionWrapper<void>* func_Wrapper = new StatefulFunctionWrapper<void>();
		func_Wrapper->m_policy_function = default_policy_func;
		
		m_explorer = new EpsilonGreedyExplorer<void>(epsilon, *func_Wrapper, default_policy_func_argument);
		
		m_default_func_wrapper = func_Wrapper;
	}

	// Port interface for Initialize Epsilon Greedy with a stateless policy function
	void Initialize_Epsilon_Greedy(
		float epsilon, 
		Stateless_Policy_Func default_policy_func)
	{
		StatelessFunctionWrapper* func_Wrapper = new StatelessFunctionWrapper();
		func_Wrapper->m_policy_function = default_policy_func;
		
		m_explorer = new EpsilonGreedyExplorer<MWT_Empty>(epsilon, *func_Wrapper, nullptr);
		
		m_default_func_wrapper = func_Wrapper;
	}

	u32 Choose_Action(feature* context_features, size_t num_features, std::string* other_context, char* unique_id, u32 length)
	{
		Context context(context_features, num_features, other_context);
		return this->Choose_Action(context, unique_id, length);
	}

	std::string Get_All_Interactions()
	{
		return m_logger->Get_All_Interactions();
	}

private:
	// TODO: App ID + Interaction ID is the unique identifier
	// Users can specify a seed and we use it to generate app id for them
	// so we can guarantee uniqueness.
	std::string Generate_App_Id()
	{
		return ""; // TODO: implement
	}

	u32 MWT::Compute_Seed(char* unique_id, u32 length)
	{
		// TODO: change return type to u64, may need to revisit this hash function
		return ::uniform_hash(unique_id, length, 0);
	}

private:
	std::string m_app_id;
	Explorer* m_explorer;
	Logger* m_logger;
	ActionSet* m_action_set;
	BaseFunctionWrapper* m_default_func_wrapper;
};